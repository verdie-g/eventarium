using System.Text.Json.Serialization;

namespace Eventarium.Core.Forge;

/// <summary>
/// Describes one configured forge source exposed through the event feed.
/// </summary>
/// <param name="Id">The stable identifier of the configured source.</param>
/// <param name="Kind">The forge kind represented by the source.</param>
/// <param name="DisplayName">The source name suitable for display to users.</param>
/// <param name="Terminology">The source-specific terminology used to describe forge concepts.</param>
public sealed record ForgeSourceDescriptor(
    string Id,
    string Kind,
    string DisplayName,
    ForgeTerminology Terminology);

/// <summary>
/// Associates a configured forge source with its latest provider status.
/// </summary>
/// <param name="Descriptor">The identity and presentation metadata of the source.</param>
/// <param name="Status">The latest reported status of the source's provider.</param>
public sealed record ForgeSourceState(
    ForgeSourceDescriptor Descriptor,
    ForgeProviderStatus Status);

/// <summary>
/// Associates a forge event with the source that delivered it.
/// </summary>
/// <param name="Id">The event identifier unique within the aggregated feed.</param>
/// <param name="SourceId">The identifier of the source that delivered the event.</param>
/// <param name="Event">The provider-independent forge event.</param>
public sealed record ForgeFeedEvent(
    string Id,
    string SourceId,
    ForgeEvent Event);

public enum ForgeFeedMessageKind
{
    Snapshot,
    Delta
}

public sealed record ForgeFeedMessage(
    ForgeFeedMessageKind Kind,
    long Revision,
    IReadOnlyList<ForgeFeedEvent> Events,
    IReadOnlyList<ForgeSourceState> Sources);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ForgeFeedMessage))]
public sealed partial class ForgeFeedJsonSerializerContext : JsonSerializerContext;
