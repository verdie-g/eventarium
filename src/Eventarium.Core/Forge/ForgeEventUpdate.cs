namespace Eventarium.Core.Forge;

/// <summary>
/// Represents a batch of events and the provider status observed with that batch.
/// </summary>
/// <param name="Events">The events emitted by the provider; the list may be empty for a status-only update.</param>
/// <param name="Status">The provider status at the time of the update.</param>
public sealed record ForgeEventUpdate(
    IReadOnlyList<ForgeEvent> Events,
    ForgeProviderStatus Status);
