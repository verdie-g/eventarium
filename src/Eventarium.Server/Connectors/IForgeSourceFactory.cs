using Eventarium.Core.Forge;
using Eventarium.Server.Configuration;

namespace Eventarium.Server.Connectors;

/// <summary>
/// Creates server-side forge event sources from named configuration entries.
/// </summary>
public interface IForgeSourceFactory
{
    /// <summary>
    /// Gets the connector name used in source configuration.
    /// </summary>
    string Connector { get; }

    /// <summary>
    /// Creates one configured source and its provider.
    /// </summary>
    /// <param name="sourceId">The stable source identifier.</param>
    /// <param name="sourceOptions">The source-specific configuration.</param>
    /// <returns>The configured source registration.</returns>
    ForgeSourceRegistration Create(
        string sourceId,
        ForgeSourceOptions sourceOptions);
}

/// <summary>
/// Associates a configured forge source with the provider that supplies its updates.
/// </summary>
/// <param name="Descriptor">The source identity and presentation metadata.</param>
/// <param name="Provider">The event provider created for the source.</param>
public sealed record ForgeSourceRegistration(
    ForgeSourceDescriptor Descriptor,
    IForgeEventProvider Provider);
