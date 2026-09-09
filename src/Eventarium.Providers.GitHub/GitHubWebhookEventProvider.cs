using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Eventarium.Core.Forge;

namespace Eventarium.Providers.GitHub;

public sealed class GitHubWebhookEventProvider : IForgeEventProvider, IGitHubWebhookReceiver
{
    private const int DeliveryHistoryCapacity = 4096;
    private const int UpdateCapacity = 256;
    private readonly Lock _gate = new();
    private readonly byte[] _secret;
    private readonly HashSet<string> _repositories;
    private readonly HashSet<string> _seenDeliveryIds = [with(StringComparer.Ordinal)];
    private readonly Queue<string> _seenDeliveryOrder = new();
    private readonly Channel<ForgeEventUpdate> _updates = Channel.CreateBounded<ForgeEventUpdate>(
        new BoundedChannelOptions(UpdateCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

    public GitHubWebhookEventProvider(string secret, IEnumerable<ForgeRepository> repositories)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        ArgumentNullException.ThrowIfNull(repositories);

        _secret = Encoding.UTF8.GetBytes(secret);
        _repositories = repositories
            .Select(repository => repository.FullName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (_repositories.Count == 0)
        {
            throw new ArgumentException("At least one repository must be configured.", nameof(repositories));
        }
    }

    public ForgeProviderDescriptor Descriptor => GitHubProviderMetadata.Descriptor;

    public async IAsyncEnumerable<ForgeEventUpdate> GetUpdatesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new ForgeEventUpdate(
            [],
            new ForgeProviderStatus(ForgeConnectionState.Waiting));

        await foreach (ForgeEventUpdate update in _updates.Reader.ReadAllAsync(cancellationToken))
        {
            yield return update;
        }
    }

    public GitHubWebhookReceiveResult Receive(GitHubWebhookDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        if (string.IsNullOrWhiteSpace(delivery.EventName) ||
            string.IsNullOrWhiteSpace(delivery.DeliveryId))
        {
            return GitHubWebhookReceiveResult.InvalidPayload;
        }

        if (!GitHubWebhookSignatureValidator.IsValid(
                delivery.Body.Span,
                delivery.Signature,
                _secret))
        {
            return GitHubWebhookReceiveResult.InvalidSignature;
        }

        GitHubWebhookMapping mapping;
        try
        {
            mapping = GitHubWebhookMapper.Map(
                delivery.EventName,
                delivery.DeliveryId,
                delivery.Body.Span,
                DateTimeOffset.UtcNow);
        }
        catch (JsonException)
        {
            return GitHubWebhookReceiveResult.InvalidPayload;
        }

        if (!mapping.IsSupported)
        {
            return GitHubWebhookReceiveResult.Ignored;
        }

        if (!mapping.IsValid)
        {
            return GitHubWebhookReceiveResult.InvalidPayload;
        }

        if (mapping.Repository is not null && !_repositories.Contains(mapping.Repository))
        {
            return GitHubWebhookReceiveResult.RepositoryNotAllowed;
        }

        if (mapping.Events.Count == 0)
        {
            return GitHubWebhookReceiveResult.Ignored;
        }

        ForgeEventUpdate update = new(
            mapping.Events,
            new ForgeProviderStatus(ForgeConnectionState.Streaming));

        lock (_gate)
        {
            if (_seenDeliveryIds.Contains(delivery.DeliveryId))
            {
                return GitHubWebhookReceiveResult.Duplicate;
            }

            if (!_updates.Writer.TryWrite(update))
            {
                return GitHubWebhookReceiveResult.QueueFull;
            }

            _ = _seenDeliveryIds.Add(delivery.DeliveryId);
            _seenDeliveryOrder.Enqueue(delivery.DeliveryId);
            while (_seenDeliveryOrder.Count > DeliveryHistoryCapacity)
            {
                _ = _seenDeliveryIds.Remove(_seenDeliveryOrder.Dequeue());
            }
        }

        return GitHubWebhookReceiveResult.Accepted;
    }
}
