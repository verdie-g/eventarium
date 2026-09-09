namespace Eventarium.Server.Configuration;

/// <summary>
/// Defines the server configuration for forge event sources.
/// </summary>
public sealed class ForgeOptions
{
    /// <summary>
    /// Gets the maximum number of feed events retained in server history.
    /// </summary>
    public int HistoryCapacity { get; init; } = 240;

    /// <summary>
    /// Gets the source configurations keyed by stable source identifier.
    /// </summary>
    public Dictionary<string, ForgeSourceOptions> Sources { get; init; } = [];
}

/// <summary>
/// Defines the configuration of one forge event source.
/// </summary>
public sealed class ForgeSourceOptions
{
    /// <summary>
    /// Gets a value indicating whether the source should be created.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the connector name used to create the source.
    /// </summary>
    public string Connector { get; init; } = "";

    /// <summary>
    /// Gets the optional source name displayed to users.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the optional connector-specific dataset name.
    /// </summary>
    public string? Dataset { get; init; }

    /// <summary>
    /// Gets the interval between events for connectors that emit scheduled events.
    /// </summary>
    public TimeSpan EventInterval { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Gets the repository identifiers in the format expected by the connector.
    /// An omitted or empty collection disables this filter.
    /// </summary>
    public IReadOnlyList<string>? Repositories { get; init; }

    /// <summary>
    /// Gets the organization identifiers in the format expected by the connector.
    /// An omitted or empty collection disables this filter.
    /// </summary>
    public IReadOnlyList<string>? Organizations { get; init; }
}
