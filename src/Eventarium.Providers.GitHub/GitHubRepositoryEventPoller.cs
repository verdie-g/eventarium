using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Eventarium.Core.Forge;

namespace Eventarium.Providers.GitHub;

public sealed class GitHubRepositoryEventPoller(
    HttpClient httpClient,
    string? accessToken = null) : IForgeRepositoryEventPoller
{
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(60);
    private static readonly string UserAgent = CreateUserAgent();

    public ForgeProviderDescriptor Descriptor => GitHubProviderMetadata.Descriptor;

    public async Task<ForgePollResult> PollAsync(
        ForgeRepository repository,
        string? entityTag,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://api.github.com/repos/{repository.Owner}/{repository.Name}/events?per_page=100");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        request.Headers.UserAgent.ParseAdd(UserAgent);
        _ = request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2026-03-10");

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        if (!string.IsNullOrWhiteSpace(entityTag))
        {
            request.Headers.IfNoneMatch.ParseAdd(entityTag);
        }

        try
        {
            using var response = await httpClient.SendAsync(request, cancellationToken);
            var rateLimit = ReadRateLimit(response);
            var pollInterval = ReadPollInterval(response);
            string? responseTag = response.Headers.ETag?.ToString() ?? entityTag;

            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                return new ForgePollResult(
                    ForgePollOutcome.NotModified,
                    [],
                    responseTag,
                    pollInterval,
                    rateLimit);
            }

            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
            {
                return new ForgePollResult(
                    ForgePollOutcome.RateLimited,
                    [],
                    responseTag,
                    pollInterval,
                    rateLimit);
            }

            if (!response.IsSuccessStatusCode)
            {
                return new ForgePollResult(
                    ForgePollOutcome.Failed,
                    [],
                    responseTag,
                    pollInterval,
                    rateLimit);
            }

            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var activityEvents = await JsonSerializer.DeserializeAsync(
                content,
                GitHubJsonSerializerContext.Default.ListGitHubActivityEvent,
                cancellationToken) ?? [];

            return new ForgePollResult(
                ForgePollOutcome.Updated,
                GitHubEventMapper.Map(activityEvents),
                responseTag,
                pollInterval,
                rateLimit);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            return new ForgePollResult(
                ForgePollOutcome.Failed,
                [],
                entityTag,
                DefaultPollInterval,
                new ForgeRateLimit(null, null));
        }
    }

    private static string CreateUserAgent()
    {
        Version version = typeof(GitHubRepositoryEventPoller).Assembly.GetName().Version ?? new Version(0, 0, 0);
        return FormattableString.Invariant(
            $"Eventarium/{version.ToString(fieldCount: 3)}");
    }

    private static TimeSpan ReadPollInterval(HttpResponseMessage response)
    {
        if (TryReadHeader(response, "X-Poll-Interval", out string value) &&
            int.TryParse(value, CultureInfo.InvariantCulture, out int seconds))
        {
            return TimeSpan.FromSeconds(Math.Max(60, seconds));
        }

        return DefaultPollInterval;
    }

    private static ForgeRateLimit ReadRateLimit(HttpResponseMessage response)
    {
        int? remaining = null;
        DateTimeOffset? resetsAt = null;

        if (TryReadHeader(response, "X-RateLimit-Remaining", out string remainingValue) &&
            int.TryParse(remainingValue, CultureInfo.InvariantCulture, out int parsedRemaining))
        {
            remaining = parsedRemaining;
        }

        if (TryReadHeader(response, "X-RateLimit-Reset", out string resetValue) &&
            long.TryParse(resetValue, CultureInfo.InvariantCulture, out long resetSeconds))
        {
            resetsAt = DateTimeOffset.FromUnixTimeSeconds(resetSeconds);
        }

        return new ForgeRateLimit(remaining, resetsAt);
    }

    private static bool TryReadHeader(
        HttpResponseMessage response,
        string name,
        out string value)
    {
        if (response.Headers.TryGetValues(name, out var values))
        {
            value = values.FirstOrDefault() ?? "";
            return !string.IsNullOrEmpty(value);
        }

        value = "";
        return false;
    }
}
