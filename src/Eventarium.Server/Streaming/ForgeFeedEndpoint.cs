using System.Globalization;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Eventarium.Core.Forge;
using Eventarium.Server.Telemetry;

namespace Eventarium.Server.Streaming;

public static class ForgeFeedEndpoint
{
    private static readonly TimeSpan KeepAliveInterval = TimeSpan.FromSeconds(15);

    public static IEndpointConventionBuilder MapForgeFeed(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/api/feed", CreateResult);

    private static Microsoft.AspNetCore.Http.HttpResults.ServerSentEventsResult<string> CreateResult(
        HttpContext context,
        ForgeFeedBroker broker,
        EventariumMetrics metrics,
        CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = "no-cache, no-store";
        context.Response.Headers.Append("X-Accel-Buffering", "no");
        long? lastRevision = long.TryParse(
            context.Request.Headers["Last-Event-ID"].ToString(),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out long revision) && revision >= 0
                ? revision
                : null;
        return TypedResults.ServerSentEvents(GetEventsAsync(broker, metrics, lastRevision, cancellationToken));
    }

    private static async IAsyncEnumerable<SseItem<string>> GetEventsAsync(
        ForgeFeedBroker broker,
        EventariumMetrics metrics,
        long? lastRevision,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using ForgeFeedSubscription subscription = broker.Subscribe(lastRevision);
        while (!cancellationToken.IsCancellationRequested)
        {
            Task<bool> messageAvailable = subscription.Messages
                .WaitToReadAsync(cancellationToken)
                .AsTask();
            Task keepAlive = Task.Delay(KeepAliveInterval, cancellationToken);
            Task completed = await Task.WhenAny(messageAvailable, keepAlive);

            if (completed == keepAlive)
            {
                yield return new SseItem<string>("{}", "keep-alive");
                continue;
            }

            if (!await messageAvailable)
            {
                yield break;
            }

            while (subscription.Messages.TryRead(out ForgeFeedMessage? message))
            {
                string eventName = message.Kind == ForgeFeedMessageKind.Snapshot ? "snapshot" : "delta";
                string json = JsonSerializer.Serialize(
                    message,
                    ForgeFeedJsonSerializerContext.Default.ForgeFeedMessage);
                metrics.RecordEventsWritten(message.Events.Count);
                yield return new SseItem<string>(json, eventName)
                {
                    EventId = message.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture)
                };
            }
        }
    }
}
