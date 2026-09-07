namespace Eventarium.Core.Forge;

/// <summary>
/// Describes a forge provider and the terminology it uses.
/// </summary>
/// <param name="DisplayName">The provider name suitable for display to users.</param>
/// <param name="Terminology">The provider-specific terminology used to describe forge concepts.</param>
public sealed record ForgeProviderDescriptor(
    string DisplayName,
    ForgeTerminology Terminology);

/// <summary>
/// Defines the user-facing terms used by a forge provider.
/// </summary>
/// <param name="ChangeRequestSingular">The singular term for a proposed set of changes.</param>
/// <param name="ChangeRequestPlural">The plural term for proposed sets of changes.</param>
public sealed record ForgeTerminology(
    string ChangeRequestSingular,
    string ChangeRequestPlural);
