using Eventarium.Core.Forge;
using Eventarium.Providers.Fake;
using Eventarium.Server.Configuration;

namespace Eventarium.Server.Connectors;

public sealed class FakeForgeSourceFactory : IForgeSourceFactory
{
    public string Connector => "Fake";

    public ForgeSourceRegistration Create(
        string sourceId,
        ForgeSourceOptions sourceOptions)
    {
        if (string.IsNullOrWhiteSpace(sourceOptions.Dataset))
        {
            throw new InvalidOperationException($"Fake source '{sourceId}' must specify a dataset.");
        }

        FakeForgeEventProvider provider = new(sourceOptions.Dataset, sourceOptions.EventInterval);
        ForgeSourceDescriptor descriptor = new(
            sourceId,
            "GitHub",
            sourceOptions.DisplayName ?? $"Recorded {provider.Repository}",
            provider.Descriptor.Terminology);
        return new ForgeSourceRegistration(descriptor, provider);
    }
}
