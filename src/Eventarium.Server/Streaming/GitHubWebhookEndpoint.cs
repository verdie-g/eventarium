using Eventarium.Providers.GitHub;
using Eventarium.Server.Connectors;
using Eventarium.Server.Telemetry;
using Microsoft.AspNetCore.Mvc;

namespace Eventarium.Server.Streaming;

public static class GitHubWebhookEndpoint
{
    private const int MaximumPayloadSize = 25 * 1024 * 1024;

    public static IEndpointRouteBuilder MapGitHubWebhooks(this IEndpointRouteBuilder endpoints)
    {
        _ = endpoints
            .MapPost("/api/webhooks/github/{sourceId}", HandleAsync)
            .WithMetadata(new RequestSizeLimitAttribute(MaximumPayloadSize));
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        string sourceId,
        HttpRequest request,
        ForgeSourceRegistry registry,
        EventariumMetrics metrics,
        ILogger<GitHubWebhookEventProvider> logger,
        CancellationToken cancellationToken)
    {
        if (!registry.TryGetSource(sourceId, out ForgeSourceRegistration? source) ||
            source.Provider is not IGitHubWebhookReceiver receiver)
        {
            return Results.NotFound();
        }

        metrics.RecordGitHubWebhookReceived();

        if (!request.HasJsonContentType())
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        if (request.ContentLength > MaximumPayloadSize)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        string eventName = request.Headers["X-GitHub-Event"].ToString();
        string deliveryId = request.Headers["X-GitHub-Delivery"].ToString();
        string signature = request.Headers["X-Hub-Signature-256"].ToString();
        if (string.IsNullOrWhiteSpace(eventName) || string.IsNullOrWhiteSpace(deliveryId))
        {
            return Results.BadRequest();
        }

        if (string.IsNullOrWhiteSpace(signature))
        {
            return Results.Unauthorized();
        }

        ReadOnlyMemory<byte> body;
        try
        {
            body = await ReadBodyAsync(
                request.Body,
                request.ContentLength,
                cancellationToken);
        }
        catch (InvalidDataException)
        {
            return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
        }

        GitHubWebhookReceiveResult result = receiver.Receive(new GitHubWebhookDelivery(
            eventName,
            deliveryId,
            signature,
            body));

        if (result == GitHubWebhookReceiveResult.QueueFull)
        {
            logger.LogError(
                "GitHub webhook queue is full for source {SourceId}; delivery {DeliveryId} was not accepted",
                sourceId,
                deliveryId);
        }

        return result switch
        {
            GitHubWebhookReceiveResult.Accepted or
            GitHubWebhookReceiveResult.Duplicate or
            GitHubWebhookReceiveResult.Ignored => Results.Accepted(),
            GitHubWebhookReceiveResult.InvalidSignature => Results.Unauthorized(),
            GitHubWebhookReceiveResult.InvalidPayload => Results.BadRequest(),
            GitHubWebhookReceiveResult.RepositoryNotAllowed => Results.NotFound(),
            GitHubWebhookReceiveResult.QueueFull => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
            _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private static async Task<ReadOnlyMemory<byte>> ReadBodyAsync(
        Stream body,
        long? contentLength,
        CancellationToken cancellationToken)
    {
        int capacity = contentLength is > 0
            ? checked((int)contentLength.Value)
            : 0;
        using MemoryStream buffer = new(capacity);
        byte[] block = new byte[81920];
        int bytesRead;
        while ((bytesRead = await body.ReadAsync(block, cancellationToken)) > 0)
        {
            if (buffer.Length + bytesRead > MaximumPayloadSize)
            {
                throw new InvalidDataException("The GitHub webhook payload exceeds the supported size.");
            }

            await buffer.WriteAsync(block.AsMemory(0, bytesRead), cancellationToken);
        }

        return buffer.GetBuffer().AsMemory(0, checked((int)buffer.Length));
    }
}
