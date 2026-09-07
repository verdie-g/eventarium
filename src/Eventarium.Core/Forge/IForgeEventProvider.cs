namespace Eventarium.Core.Forge;

/// <summary>
/// Provides a continuous sequence of forge events and connection status updates.
/// </summary>
/// <remarks>
/// Implementations may receive events through polling, webhooks, or another transport.
/// </remarks>
public interface IForgeEventProvider
{
    /// <summary>
    /// Gets the provider name and forge-specific terminology.
    /// </summary>
    ForgeProviderDescriptor Descriptor { get; }

    /// <summary>
    /// Asynchronously streams event batches and provider status changes.
    /// </summary>
    /// <param name="cancellationToken">The token used to stop receiving updates.</param>
    /// <returns>An asynchronous sequence that continues until cancellation or provider completion.</returns>
    IAsyncEnumerable<ForgeEventUpdate> GetUpdatesAsync(CancellationToken cancellationToken);
}
