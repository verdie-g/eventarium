namespace Eventarium.Core.Forge;

/// <summary>
/// Performs individual pull-based requests for activity from a forge repository.
/// </summary>
/// <remarks>
/// This contract exposes polling concerns such as entity tags, rate limits, and recommended poll intervals.
/// Push-based sources such as webhook receivers should implement <see cref="IForgeEventProvider"/> instead.
/// </remarks>
public interface IForgeRepositoryEventPoller
{
    /// <summary>
    /// Gets the provider name and forge-specific terminology.
    /// </summary>
    ForgeProviderDescriptor Descriptor { get; }

    /// <summary>
    /// Requests the latest activity for one repository.
    /// </summary>
    /// <param name="repository">The repository whose activity should be requested.</param>
    /// <param name="entityTag">
    /// The entity tag from the previous response, or <see langword="null"/> for an unconditional request.
    /// </param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>
    /// A task whose result contains the request outcome, mapped events, response entity tag,
    /// recommended polling interval, and current rate-limit information.
    /// </returns>
    Task<ForgePollResult> PollAsync(
        ForgeRepository repository,
        string? entityTag,
        CancellationToken cancellationToken);
}

/// <summary>
/// Identifies the outcome of a repository polling request.
/// </summary>
public enum ForgePollOutcome
{
    /// <summary>
    /// The request succeeded and returned the latest repository activity.
    /// </summary>
    Updated,

    /// <summary>
    /// The repository activity has not changed since the supplied entity tag.
    /// </summary>
    NotModified,

    /// <summary>
    /// The provider rejected the request because its request quota was exhausted.
    /// </summary>
    RateLimited,

    /// <summary>
    /// The request or response processing failed.
    /// </summary>
    Failed
}

/// <summary>
/// Contains the result of polling one forge repository for activity.
/// </summary>
/// <param name="Outcome">The outcome of the polling request.</param>
/// <param name="Events">The mapped events returned by the request, or an empty list when none were returned.</param>
/// <param name="EntityTag">
/// The entity tag to send with the next request, or <see langword="null"/> when no tag is available.
/// </param>
/// <param name="PollInterval">The minimum interval recommended before polling this repository again.</param>
/// <param name="RateLimit">The request quota information observed in the response.</param>
public sealed record ForgePollResult(
    ForgePollOutcome Outcome,
    IReadOnlyList<ForgeEvent> Events,
    string? EntityTag,
    TimeSpan PollInterval,
    ForgeRateLimit RateLimit);
