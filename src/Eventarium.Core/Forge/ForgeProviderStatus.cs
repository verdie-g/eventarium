namespace Eventarium.Core.Forge;

/// <summary>
/// Describes the operational state of a forge event provider.
/// </summary>
public enum ForgeConnectionState
{
    /// <summary>
    /// The provider has been registered but has not started connecting.
    /// </summary>
    Starting,

    /// <summary>
    /// The provider is establishing a connection or making its initial requests.
    /// </summary>
    Connecting,

    /// <summary>
    /// The provider is operating normally and can deliver events.
    /// </summary>
    Streaming,

    /// <summary>
    /// The provider is operating normally but has not observed matching activity.
    /// </summary>
    Waiting,

    /// <summary>
    /// The provider is paused because its request quota has been exhausted.
    /// </summary>
    RateLimited,

    /// <summary>
    /// The provider is temporarily unable to obtain events.
    /// </summary>
    Unavailable,
}

/// <summary>
/// Reports the current operational and rate-limit state of a forge event provider.
/// </summary>
/// <param name="ConnectionState">The provider's current operational state.</param>
/// <param name="RateLimit">
/// The latest known request quota, or <see langword="null"/> when no rate-limit information is available.
/// </param>
public sealed record ForgeProviderStatus(
    ForgeConnectionState ConnectionState,
    ForgeRateLimit? RateLimit = null);
