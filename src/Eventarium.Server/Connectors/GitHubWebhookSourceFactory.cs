using Eventarium.Core.Forge;
using Eventarium.Providers.GitHub;
using Eventarium.Server.Configuration;

namespace Eventarium.Server.Connectors;

public sealed class GitHubWebhookSourceFactory(string? webhookSecret) : IForgeSourceFactory
{
    public string Connector => "GitHubWebhook";

    public ForgeSourceRegistration Create(
        string sourceId,
        ForgeSourceOptions sourceOptions)
    {
        if (sourceOptions.Repositories.Count == 0)
        {
            throw new InvalidOperationException(
                $"GitHub webhook source '{sourceId}' must configure at least one repository.");
        }

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new InvalidOperationException(
                $"GitHub webhook source '{sourceId}' requires the GITHUB_WEBHOOK_SECRET environment variable.");
        }

        ForgeRepository[] repositories = sourceOptions.Repositories
            .Select(repository => ParseRepository(sourceId, repository))
            .ToArray();
        GitHubWebhookEventProvider provider = new(webhookSecret, repositories);
        ForgeSourceDescriptor descriptor = new(
            sourceId,
            "GitHub",
            sourceOptions.DisplayName ?? sourceId,
            provider.Descriptor.Terminology);
        return new ForgeSourceRegistration(descriptor, provider);
    }

    private static ForgeRepository ParseRepository(string sourceId, string value)
    {
        string[] segments = value.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 2)
        {
            throw new InvalidOperationException(
                $"Repository '{value}' in GitHub webhook source '{sourceId}' must use the 'owner/name' format.");
        }

        return new ForgeRepository(segments[0], segments[1]);
    }
}
