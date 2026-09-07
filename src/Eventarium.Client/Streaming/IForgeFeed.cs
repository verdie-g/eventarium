using Eventarium.Core.Forge;

namespace Eventarium.Client.Streaming;

/// <summary>
/// Exposes the client-side state received from the Eventarium server feed.
/// </summary>
public interface IForgeFeed
{
    /// <summary>
    /// Occurs when the connection, source states, or released events change.
    /// </summary>
    event Action? Changed;

    /// <summary>
    /// Gets a value indicating whether the server event stream is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the events currently retained for presentation.
    /// </summary>
    IReadOnlyList<ForgeFeedEvent> ReleasedEvents { get; }

    /// <summary>
    /// Gets the configured forge sources and their latest statuses.
    /// </summary>
    IReadOnlyList<ForgeSourceState> Sources { get; }
}
