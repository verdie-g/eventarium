using System.Runtime.CompilerServices;
using Eventarium.Core.Forge;

namespace Eventarium.Core.Application;

public sealed class PollingForgeEventProvider(
    IForgeRepositoryEventPoller poller,
    IReadOnlyList<ForgeRepository>? configuredRepositories = null) : IForgeEventProvider
{
    // One repository every 75 seconds keeps steady-state polling below the 60-request hourly limit.
    private static readonly TimeSpan RepositoryPollSpacing = TimeSpan.FromSeconds(75);
    private static readonly TimeSpan InitialRepositorySpacing = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan DefaultRateLimitDelay = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MinimumRateLimitDelay = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan RateLimitResetGracePeriod = TimeSpan.FromSeconds(2);
    private readonly Dictionary<string, string?> _entityTags = [];
    private readonly Dictionary<string, DateTimeOffset> _nextPollAt = [];

    public ForgeProviderDescriptor Descriptor => poller.Descriptor;

    public async IAsyncEnumerable<ForgeEventUpdate> GetUpdatesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IReadOnlyList<ForgeRepository> repositories = configuredRepositories ?? ForgeRepository.Defaults;
        List<ForgeEvent> initialEvents = [];
        DateTimeOffset? initialRateLimitReset = null;
        ForgeRateLimit? rateLimit = null;
        bool initiallyRateLimited = false;
        bool receivedSuccessfulResponse = false;

        for (int index = 0; index < repositories.Count; index++)
        {
            yield return new ForgeEventUpdate(
                [],
                new ForgeProviderStatus(ForgeConnectionState.Connecting, rateLimit));

            ForgePollResult result = await PollRepositoryAsync(repositories[index], cancellationToken);
            rateLimit = result.RateLimit;

            if (result.Outcome == ForgePollOutcome.Updated)
            {
                initialEvents.AddRange(result.Events);
            }

            if (result.Outcome is ForgePollOutcome.Updated or ForgePollOutcome.NotModified)
            {
                receivedSuccessfulResponse = true;
            }

            if (result.Outcome == ForgePollOutcome.RateLimited)
            {
                initiallyRateLimited = true;
                initialRateLimitReset = result.RateLimit.ResetsAt;
                break;
            }

            if (index < repositories.Count - 1)
            {
                await Task.Delay(InitialRepositorySpacing, cancellationToken);
            }
        }

        ForgeConnectionState initialState = initiallyRateLimited
            ? ForgeConnectionState.RateLimited
            : receivedSuccessfulResponse
                ? initialEvents.Count > 0
                    ? ForgeConnectionState.Streaming
                    : ForgeConnectionState.Waiting
                : ForgeConnectionState.Unavailable;

        yield return new ForgeEventUpdate(
            initialEvents
                .OrderByDescending(forgeEvent => forgeEvent.CreatedAt)
                .ThenBy(forgeEvent => forgeEvent.Repository)
                .ToArray(),
            new ForgeProviderStatus(initialState, rateLimit));

        if (initiallyRateLimited)
        {
            await DelayUntilResetAsync(initialRateLimitReset, cancellationToken);
        }

        int repositoryIndex = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(RepositoryPollSpacing, cancellationToken);
            ForgeRepository repository = repositories[repositoryIndex];
            repositoryIndex = (repositoryIndex + 1) % repositories.Count;

            if (_nextPollAt.TryGetValue(repository.FullName, out DateTimeOffset nextPollAt) &&
                nextPollAt > DateTimeOffset.UtcNow)
            {
                continue;
            }

            ForgePollResult result = await PollRepositoryAsync(repository, cancellationToken);
            rateLimit = result.RateLimit;

            switch (result.Outcome)
            {
                case ForgePollOutcome.Updated:
                    yield return new ForgeEventUpdate(
                        result.Events.OrderByDescending(forgeEvent => forgeEvent.CreatedAt).ToArray(),
                        new ForgeProviderStatus(ForgeConnectionState.Streaming, rateLimit));
                    break;
                case ForgePollOutcome.NotModified:
                    yield return new ForgeEventUpdate(
                        [],
                        new ForgeProviderStatus(ForgeConnectionState.Streaming, rateLimit));
                    break;
                case ForgePollOutcome.RateLimited:
                    yield return new ForgeEventUpdate(
                        [],
                        new ForgeProviderStatus(ForgeConnectionState.RateLimited, rateLimit));
                    await DelayUntilResetAsync(result.RateLimit.ResetsAt, cancellationToken);
                    break;
                case ForgePollOutcome.Failed:
                    yield return new ForgeEventUpdate(
                        [],
                        new ForgeProviderStatus(ForgeConnectionState.Unavailable, rateLimit));
                    break;
            }
        }
    }

    private static Task DelayUntilResetAsync(
        DateTimeOffset? resetsAt,
        CancellationToken cancellationToken)
    {
        TimeSpan delay = resetsAt is null
            ? DefaultRateLimitDelay
            : resetsAt.Value - DateTimeOffset.UtcNow + RateLimitResetGracePeriod;

        return Task.Delay(delay < MinimumRateLimitDelay ? MinimumRateLimitDelay : delay, cancellationToken);
    }

    private async Task<ForgePollResult> PollRepositoryAsync(
        ForgeRepository repository,
        CancellationToken cancellationToken)
    {
        string? entityTag = _entityTags.GetValueOrDefault(repository.FullName);
        ForgePollResult result = await poller.PollAsync(repository, entityTag, cancellationToken);
        _entityTags[repository.FullName] = result.EntityTag;
        _nextPollAt[repository.FullName] = DateTimeOffset.UtcNow + result.PollInterval;
        return result;
    }
}
