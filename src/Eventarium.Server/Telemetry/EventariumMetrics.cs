using System.Diagnostics.Metrics;

namespace Eventarium.Server.Telemetry;

public sealed class EventariumMetrics : IDisposable
{
    public const string MeterName = "Eventarium.Server";

    private readonly Meter _meter = new(MeterName);
    private readonly Counter<long> _githubWebhooksReceived;
    private readonly Counter<long> _eventsWritten;

    public EventariumMetrics()
    {
        _githubWebhooksReceived = _meter.CreateCounter<long>(
            "eventarium.provider.github.webhooks.received",
            "{webhook}",
            "The number of GitHub webhooks received by an Eventarium provider.");
        _eventsWritten = _meter.CreateCounter<long>(
            "eventarium.events.written",
            "{event}",
            "The number of Eventarium events written to server-sent event streams.");
    }

    public void RecordGitHubWebhookReceived() => _githubWebhooksReceived.Add(1);

    public void RecordEventsWritten(int count)
    {
        if (count > 0)
        {
            _eventsWritten.Add(count);
        }
    }

    public void Dispose() => _meter.Dispose();
}
