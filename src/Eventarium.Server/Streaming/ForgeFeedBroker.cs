using System.Threading.Channels;
using Eventarium.Core.Forge;
using Eventarium.Server.Connectors;

namespace Eventarium.Server.Streaming;

public sealed class ForgeFeedBroker
{
    private const int ReplayCapacity = 32;
    private const int SubscriberCapacity = ReplayCapacity * 2;
    private readonly Lock _gate = new();
    private readonly int _historyCapacity;
    private readonly List<ForgeFeedEvent> _history = [];
    private readonly Queue<ForgeFeedMessage> _replay = new();
    private readonly HashSet<string> _seenEventIds = new(StringComparer.Ordinal);
    private readonly Queue<string> _seenEventOrder = new();
    private readonly Dictionary<string, ForgeSourceState> _sources;
    private readonly Dictionary<Guid, Channel<ForgeFeedMessage>> _subscribers = [];
    private long _revision;

    public ForgeFeedBroker(ForgeSourceRegistry registry)
    {
        _historyCapacity = registry.HistoryCapacity;
        _sources = registry.Sources.ToDictionary(
            source => source.Descriptor.Id,
            source => new ForgeSourceState(
                source.Descriptor,
                new ForgeProviderStatus(ForgeConnectionState.Starting)),
            StringComparer.Ordinal);
    }

    public ForgeFeedSubscription Subscribe(long? lastRevision = null)
    {
        lock (_gate)
        {
            Guid subscriptionId = Guid.NewGuid();
            Channel<ForgeFeedMessage> channel = Channel.CreateBounded<ForgeFeedMessage>(
                new BoundedChannelOptions(SubscriberCapacity)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

            foreach (ForgeFeedMessage message in GetInitialMessages(lastRevision))
            {
                _ = channel.Writer.TryWrite(message);
            }

            _subscribers.Add(subscriptionId, channel);
            return new ForgeFeedSubscription(channel.Reader, () => Unsubscribe(subscriptionId));
        }
    }

    public void Publish(ForgeSourceRegistration source, ForgeEventUpdate update)
    {
        lock (_gate)
        {
            List<ForgeFeedEvent> addedEvents = [];
            foreach (ForgeEvent forgeEvent in update.Events)
            {
                string eventId = $"{source.Descriptor.Id}:{forgeEvent.Id}";
                if (!_seenEventIds.Add(eventId))
                {
                    continue;
                }

                ForgeFeedEvent feedEvent = new(eventId, source.Descriptor.Id, forgeEvent);
                addedEvents.Add(feedEvent);
                _history.Add(feedEvent);
                _seenEventOrder.Enqueue(eventId);
            }

            if (_history.Count > _historyCapacity)
            {
                _history.RemoveRange(0, _history.Count - _historyCapacity);
            }

            TrimSeenEvents();
            ForgeSourceState sourceState = new(source.Descriptor, update.Status);
            _sources[source.Descriptor.Id] = sourceState;
            ForgeFeedMessage delta = new(
                ForgeFeedMessageKind.Delta,
                ++_revision,
                addedEvents,
                [sourceState]);
            _replay.Enqueue(delta);
            while (_replay.Count > ReplayCapacity)
            {
                _ = _replay.Dequeue();
            }

            Broadcast(delta);
        }
    }

    private ForgeFeedMessage[] GetInitialMessages(long? lastRevision)
    {
        if (lastRevision is >= 0 && lastRevision <= _revision)
        {
            if (lastRevision == _revision)
            {
                return [];
            }

            ForgeFeedMessage[] replay = _replay
                .Where(message => message.Revision > lastRevision)
                .ToArray();
            if (replay.Length > 0 &&
                replay[0].Revision == lastRevision + 1 &&
                replay[^1].Revision == _revision)
            {
                return replay;
            }
        }

        return
        [
            new ForgeFeedMessage(
                ForgeFeedMessageKind.Snapshot,
                _revision,
                _history.ToArray(),
                _sources.Values.ToArray())
        ];
    }

    private void Broadcast(ForgeFeedMessage message)
    {
        List<Guid> disconnectedSubscribers = [];
        foreach ((Guid subscriptionId, Channel<ForgeFeedMessage> channel) in _subscribers)
        {
            if (!channel.Writer.TryWrite(message))
            {
                _ = channel.Writer.TryComplete();
                disconnectedSubscribers.Add(subscriptionId);
            }
        }

        foreach (Guid subscriptionId in disconnectedSubscribers)
        {
            _ = _subscribers.Remove(subscriptionId);
        }
    }

    private void TrimSeenEvents()
    {
        int maximumSeenEvents = _historyCapacity * 8;
        while (_seenEventOrder.Count > maximumSeenEvents)
        {
            _ = _seenEventIds.Remove(_seenEventOrder.Dequeue());
        }
    }

    private void Unsubscribe(Guid subscriptionId)
    {
        lock (_gate)
        {
            if (_subscribers.Remove(subscriptionId, out Channel<ForgeFeedMessage>? channel))
            {
                _ = channel.Writer.TryComplete();
            }
        }
    }
}

public sealed class ForgeFeedSubscription(
    ChannelReader<ForgeFeedMessage> messages,
    Action unsubscribe) : IAsyncDisposable
{
    public ChannelReader<ForgeFeedMessage> Messages => messages;

    public ValueTask DisposeAsync()
    {
        unsubscribe();
        return ValueTask.CompletedTask;
    }
}
