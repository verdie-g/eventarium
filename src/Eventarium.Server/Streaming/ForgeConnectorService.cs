using Eventarium.Core.Forge;
using Eventarium.Server.Connectors;

namespace Eventarium.Server.Streaming;

public sealed class ForgeConnectorService(
    ForgeSourceRegistry registry,
    ForgeFeedBroker broker,
    ILogger<ForgeConnectorService> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.WhenAll(registry.Sources.Select(source => ConsumeAsync(source, stoppingToken)));

    private async Task ConsumeAsync(
        ForgeSourceRegistration source,
        CancellationToken cancellationToken)
    {
        try
        {
            await foreach (ForgeEventUpdate update in source.Provider.GetUpdatesAsync(cancellationToken))
            {
                broker.Publish(source, update);
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                broker.Publish(source, new ForgeEventUpdate(
                    [],
                    new ForgeProviderStatus(ForgeConnectionState.Unavailable)));
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Forge source {SourceId} stopped unexpectedly", source.Descriptor.Id);
            broker.Publish(source, new ForgeEventUpdate(
                [],
                new ForgeProviderStatus(ForgeConnectionState.Unavailable)));
        }
    }
}
