using System.Diagnostics.CodeAnalysis;
using Eventarium.Server.Configuration;
using Microsoft.Extensions.Options;

namespace Eventarium.Server.Connectors;

public sealed class ForgeSourceRegistry
{
    private readonly Dictionary<string, ForgeSourceRegistration> _sourcesById;

    public ForgeSourceRegistry(
        IOptions<ForgeOptions> options,
        IEnumerable<IForgeSourceFactory> factories)
    {
        ForgeOptions forgeOptions = options.Value;
        Dictionary<string, IForgeSourceFactory> factoriesByName = factories.ToDictionary(
            factory => factory.Connector,
            StringComparer.OrdinalIgnoreCase);
        List<ForgeSourceRegistration> registrations = [];

        foreach ((string sourceId, ForgeSourceOptions sourceOptions) in forgeOptions.Sources)
        {
            if (!sourceOptions.Enabled)
            {
                continue;
            }

            if (!factoriesByName.TryGetValue(sourceOptions.Connector, out IForgeSourceFactory? factory))
            {
                throw new InvalidOperationException(
                    $"Forge source '{sourceId}' uses unknown connector '{sourceOptions.Connector}'.");
            }

            registrations.Add(factory.Create(sourceId, sourceOptions));
        }

        if (registrations.Count == 0)
        {
            throw new InvalidOperationException("At least one enabled forge source must be configured.");
        }

        Sources = registrations;
        _sourcesById = registrations.ToDictionary(
            registration => registration.Descriptor.Id,
            StringComparer.Ordinal);
        HistoryCapacity = Math.Max(1, forgeOptions.HistoryCapacity);
    }

    public int HistoryCapacity { get; }

    public IReadOnlyList<ForgeSourceRegistration> Sources { get; }

    public bool TryGetSource(
        string sourceId,
        [NotNullWhen(true)] out ForgeSourceRegistration? source) =>
        _sourcesById.TryGetValue(sourceId, out source);
}
