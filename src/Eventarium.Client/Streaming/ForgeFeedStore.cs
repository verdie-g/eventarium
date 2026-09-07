using System.Threading.Channels;
using Eventarium.Core.Forge;

namespace Eventarium.Client.Streaming;

public sealed class ForgeFeedStore : IForgeFeed
{
    private const int MaximumPendingEvents = 240;
    private const int MaximumReleasedEventHistory = 32;
    private const int MinimumReleaseDelayMilliseconds = 2_600;
    private const int MaximumReleaseDelayMilliseconds = 4_700;
    private static readonly TimeSpan InitialReleaseDelay = TimeSpan.FromMilliseconds(900);
    private readonly Lock _gate = new();
    private readonly Channel<ForgeFeedEvent> _pending = Channel.CreateBounded<ForgeFeedEvent>(
        new BoundedChannelOptions(MaximumPendingEvents)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    private readonly HashSet<string> _seenEventIds = new(StringComparer.Ordinal);
    private readonly Queue<string> _seenEventOrder = new();
    private readonly Dictionary<string, ForgeSourceState> _sources = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _stopping = new();
    private IReadOnlyList<ForgeFeedEvent> _releasedEvents = [];
    private Task? _releaseTask;
    private long _revision = -1;

    public event Action? Changed;

    public bool IsConnected { get; private set; }

    public IReadOnlyList<ForgeFeedEvent> ReleasedEvents
    {
        get
        {
            lock (_gate)
            {
                return _releasedEvents;
            }
        }
    }

    public IReadOnlyList<ForgeSourceState> Sources
    {
        get
        {
            lock (_gate)
            {
                return _sources.Values.ToArray();
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _releaseTask ??= ReleaseLoopAsync(_stopping.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stopping.CancelAsync();
        if (_releaseTask is not null)
        {
            try
            {
                await _releaseTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public void Apply(ForgeFeedMessage message)
    {
        bool shouldNotify;
        lock (_gate)
        {
            if (message.Kind == ForgeFeedMessageKind.Delta && message.Revision <= _revision)
            {
                return;
            }

            shouldNotify = message.Kind == ForgeFeedMessageKind.Snapshot;
            if (message.Kind == ForgeFeedMessageKind.Snapshot)
            {
                _sources.Clear();
            }

            foreach (ForgeSourceState source in message.Sources)
            {
                shouldNotify |= !_sources.TryGetValue(source.Descriptor.Id, out ForgeSourceState? current) ||
                    current != source;
                _sources[source.Descriptor.Id] = source;
            }

            foreach (ForgeFeedEvent forgeEvent in message.Events)
            {
                Enqueue(forgeEvent);
            }

            _revision = message.Revision;
        }

        if (shouldNotify)
        {
            Changed?.Invoke();
        }
    }

    public void SetConnected(bool isConnected)
    {
        if (IsConnected == isConnected)
        {
            return;
        }

        IsConnected = isConnected;
        Changed?.Invoke();
    }

    private void Enqueue(ForgeFeedEvent forgeEvent)
    {
        if (_seenEventIds.Contains(forgeEvent.Id) || !_pending.Writer.TryWrite(forgeEvent))
        {
            return;
        }

        _ = _seenEventIds.Add(forgeEvent.Id);
        _seenEventOrder.Enqueue(forgeEvent.Id);
        int maximumSeenEvents = MaximumPendingEvents * 8;
        while (_seenEventOrder.Count > maximumSeenEvents)
        {
            _ = _seenEventIds.Remove(_seenEventOrder.Dequeue());
        }
    }

    private async Task ReleaseLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(InitialReleaseDelay, cancellationToken);
            await foreach (ForgeFeedEvent forgeEvent in _pending.Reader.ReadAllAsync(cancellationToken))
            {
                lock (_gate)
                {
                    _releasedEvents = _releasedEvents
                        .Append(forgeEvent)
                        .TakeLast(MaximumReleasedEventHistory)
                        .ToArray();
                }

                Changed?.Invoke();
                await Task.Delay(
                    Random.Shared.Next(
                        MinimumReleaseDelayMilliseconds,
                        MaximumReleaseDelayMilliseconds + 1),
                    cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
