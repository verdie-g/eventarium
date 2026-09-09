using Eventarium.Core.Application;
using Eventarium.Core.Forge;
using Eventarium.Providers.GitHub;
using Eventarium.Server.Configuration;

namespace Eventarium.Server.Connectors;

public sealed class GitHubForgeSourceFactory(
    IHttpClientFactory httpClientFactory,
    string? accessToken) : IForgeSourceFactory
{
    public string Connector => "GitHubPolling";

    public ForgeSourceRegistration Create(
        string sourceId,
        ForgeSourceOptions sourceOptions)
    {
        if (sourceOptions.Repositories is not { Count: > 0 } repositories)
        {
            throw new InvalidOperationException(
                $"GitHub source '{sourceId}' must configure at least one repository.");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException(
                $"GitHub source '{sourceId}' requires the GITHUB_TOKEN environment variable.");
        }

        ForgeRepository[] parsedRepositories = repositories
            .Select(repository => ParseRepository(sourceId, repository))
            .ToArray();
        GitHubRepositoryEventPoller poller = new(
            httpClientFactory.CreateClient(nameof(GitHubRepositoryEventPoller)),
            accessToken);
        PollingForgeEventProvider provider = new(poller, parsedRepositories);
        ForgeSourceDescriptor descriptor = new(
            sourceId,
            "GitHub",
            sourceOptions.DisplayName ?? sourceId,
            provider.Descriptor.Terminology);
        return new ForgeSourceRegistration(descriptor, provider);
    }

    private static ForgeRepository ParseRepository(string sourceId, string value)
    {
        string[] segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
        {
            throw new InvalidOperationException(
                $"Repository '{value}' in GitHub source '{sourceId}' must use the 'owner/name' format.");
        }

        return new ForgeRepository(segments[0], segments[1]);
    }
}
