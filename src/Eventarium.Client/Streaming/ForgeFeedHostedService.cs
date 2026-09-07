using System.Net.ServerSentEvents;
using System.Text.Json;
using Eventarium.Core.Forge;
using Microsoft.Extensions.Hosting;

namespace Eventarium.Client.Streaming;

public sealed class ForgeFeedHostedService(
    HttpClient httpClient,
    ForgeFeedStore feed,
    ILogger<ForgeFeedHostedService> logger) : IHostedService
{
    private static readonly TimeSpan DefaultReconnectDelay = TimeSpan.FromSeconds(3);
    private readonly CancellationTokenSource _stopping = new();
    private Task? _receiveTask;
    private string? _lastEventId;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = feed.StartAsync(cancellationToken);
        _receiveTask = ReceiveLoopAsync(_stopping.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stopping.CancelAsync();
        if (_receiveTask is not null)
        {
            try
            {
                await _receiveTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        await feed.StopAsync(cancellationToken);
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TimeSpan reconnectDelay = DefaultReconnectDelay;
            try
            {
                using HttpRequestMessage request = new(HttpMethod.Get, "api/feed");
                if (!string.IsNullOrWhiteSpace(_lastEventId))
                {
                    _ = request.Headers.TryAddWithoutValidation("Last-Event-ID", _lastEventId);
                }

                using HttpResponseMessage response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                _ = response.EnsureSuccessStatusCode();
                feed.SetConnected(true);

                await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
                SseParser<string> parser = SseParser.Create(content);
                await foreach (SseItem<string> item in parser.EnumerateAsync(cancellationToken))
                {
                    if (item.EventType is "snapshot" or "delta")
                    {
                        ApplyMessage(item.Data);
                        _lastEventId = item.EventId ?? _lastEventId;
                    }
                }

                if (parser.ReconnectionInterval != Timeout.InfiniteTimeSpan)
                {
                    reconnectDelay = parser.ReconnectionInterval;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception) when (exception is HttpRequestException or IOException or JsonException)
            {
                logger.LogWarning(exception, "The forge feed connection was interrupted");
            }
            finally
            {
                feed.SetConnected(false);
            }

            await Task.Delay(reconnectDelay, cancellationToken);
        }
    }

    private void ApplyMessage(string json)
    {
        ForgeFeedMessage? message = JsonSerializer.Deserialize(
            json,
            ForgeFeedJsonSerializerContext.Default.ForgeFeedMessage);
        if (message is not null)
        {
            feed.Apply(message);
        }
    }
}
