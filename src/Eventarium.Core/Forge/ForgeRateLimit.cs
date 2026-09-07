namespace Eventarium.Core.Forge;

/// <summary>
/// Describes the latest known request quota for a forge provider.
/// </summary>
/// <param name="Remaining">
/// The number of requests remaining in the current quota window, or <see langword="null"/> when unknown.
/// </param>
/// <param name="ResetsAt">
/// The date and time at which the quota is expected to reset, or <see langword="null"/> when unknown.
/// </param>
public sealed record ForgeRateLimit(int? Remaining, DateTimeOffset? ResetsAt);
