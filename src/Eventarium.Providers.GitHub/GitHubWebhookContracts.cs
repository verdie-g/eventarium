namespace Eventarium.Providers.GitHub;

/// <summary>
/// Represents the GitHub-specific HTTP data needed to process one webhook delivery.
/// </summary>
public sealed record GitHubWebhookDelivery(
    string EventName,
    string DeliveryId,
    string? Signature,
    ReadOnlyMemory<byte> Body);

/// <summary>
/// Describes how a GitHub webhook delivery was handled.
/// </summary>
public enum GitHubWebhookReceiveResult
{
    Accepted,
    Duplicate,
    Ignored,
    InvalidSignature,
    InvalidPayload,
    RepositoryNotAllowed,
    QueueFull
}

/// <summary>
/// Receives authenticated GitHub webhook deliveries for a configured forge source.
/// </summary>
public interface IGitHubWebhookReceiver
{
    GitHubWebhookReceiveResult Receive(GitHubWebhookDelivery delivery);
}
