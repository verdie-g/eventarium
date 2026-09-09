using Eventarium.Client.Streaming;
using Eventarium.Core.Forge;

namespace Eventarium.Client.Presentation;

public sealed class HomePageViewModel : IDisposable
{
    private readonly IForgeFeed _feed;
    private readonly DisplayMotionProfile _displayMotionProfile;
    private readonly HashSet<string> _displayedEventIds = [with(StringComparer.Ordinal)];
    private IReadOnlyList<DisplayedForgeEvent> _activeEvents = [];

    public HomePageViewModel(IForgeFeed feed, DisplayMotionProfile displayMotionProfile)
    {
        _feed = feed;
        _displayMotionProfile = displayMotionProfile;
        _feed.Changed += HandleFeedChanged;
        Refresh();
    }

    public event Action? Changed;

    public IReadOnlyList<DisplayedForgeEvent> ActiveEvents => _activeEvents;

    public IReadOnlyList<ForgeSourceState> Sources => _feed.Sources;

    public string Status => GetStatus();

    public string EventRegionLabel => $"Activity from {_feed.Sources.Count} forge sources";

    public string OpenEventTitle => "Open this event on its forge";

    public void SetViewportWidth(double viewportWidth) => _displayMotionProfile.SetViewportWidth(viewportWidth);

    public void CompleteFlight(string displayId)
    {
        DisplayedForgeEvent[] remainingEvents = _activeEvents
            .Where(displayedEvent => displayedEvent.DisplayId != displayId)
            .ToArray();
        if (remainingEvents.Length == _activeEvents.Count)
        {
            return;
        }

        _activeEvents = remainingEvents;
        Changed?.Invoke();
    }

    public void Dispose()
    {
        _feed.Changed -= HandleFeedChanged;
    }

    private string GetStatus()
    {
        if (!_feed.IsConnected)
        {
            return "Connecting to the Eventarium server";
        }

        IReadOnlyList<ForgeSourceState> sources = _feed.Sources;
        if (sources.Count == 0)
        {
            return "Waiting for configured forge sources";
        }

        if (sources.Count == 1)
        {
            return GetSourceStatus(sources[0]);
        }

        int streamingSources = sources.Count(source =>
            source.Status.ConnectionState == ForgeConnectionState.Streaming);
        return streamingSources == sources.Count
            ? $"Streaming {sources.Count} forge sources"
            : $"Streaming {streamingSources} of {sources.Count} forge sources";
    }

    private static string GetSourceStatus(ForgeSourceState source)
    {
        string sourceName = source.Descriptor.DisplayName;
        return source.Status.ConnectionState switch
        {
            ForgeConnectionState.Starting => $"Starting {sourceName}",
            ForgeConnectionState.Connecting => $"Connecting to {sourceName}",
            ForgeConnectionState.Streaming => $"Streaming {sourceName} activity",
            ForgeConnectionState.Waiting => $"Waiting for matching {sourceName} activity",
            ForgeConnectionState.RateLimited => GetRateLimitStatus(sourceName, source.Status.RateLimit?.ResetsAt),
            ForgeConnectionState.Unavailable => $"{sourceName} activity temporarily unavailable",
            _ => $"Waiting for {sourceName} activity"
        };
    }

    private static string GetRateLimitStatus(string sourceName, DateTimeOffset? resetsAt)
    {
        string suffix = resetsAt is null
            ? string.Empty
            : $" · resumes {resetsAt.Value.ToLocalTime():HH:mm}";
        return $"{sourceName} rate limited{suffix}";
    }

    private ForgeSourceState? GetSource(string sourceId) =>
        _feed.Sources.FirstOrDefault(source => source.Descriptor.Id == sourceId);

    private void HandleFeedChanged()
    {
        Refresh();
        Changed?.Invoke();
    }

    private void Refresh()
    {
        IReadOnlyList<ForgeFeedEvent> releasedEvents = _feed.ReleasedEvents;
        HashSet<string> retainedEventIds = releasedEvents
            .Select(feedEvent => feedEvent.Id)
            .Concat(_activeEvents.Select(displayedEvent => displayedEvent.EventId))
            .ToHashSet(StringComparer.Ordinal);
        _displayedEventIds.IntersectWith(retainedEventIds);

        List<DisplayedForgeEvent> activeEvents = [.. _activeEvents];
        foreach (ForgeFeedEvent feedEvent in releasedEvents)
        {
            if (!_displayedEventIds.Add(feedEvent.Id))
            {
                continue;
            }

            activeEvents.Add(DisplayedForgeEvent.Create(
                feedEvent,
                GetSource(feedEvent.SourceId)?.Descriptor,
                _displayMotionProfile.FlightDurationScale));
        }

        _activeEvents = activeEvents;
    }
}
