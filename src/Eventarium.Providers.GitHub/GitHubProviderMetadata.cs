using Eventarium.Core.Forge;

namespace Eventarium.Providers.GitHub;

internal static class GitHubProviderMetadata
{
    public const string ChangeRequestSingular = "pull request";
    public const string ChangeRequestPlural = "pull requests";

    public static ForgeProviderDescriptor Descriptor { get; } = new(
        "GitHub",
        new ForgeTerminology(ChangeRequestSingular, ChangeRequestPlural));
}
