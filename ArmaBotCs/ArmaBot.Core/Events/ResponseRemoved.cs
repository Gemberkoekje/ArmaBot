using Remora.Rest.Core;

namespace ArmaBot.Core.Events;

/// <summary>
/// Represents an event indicating that a user's response has been removed from a mission or activity.
/// </summary>
public sealed class ResponseRemoved
{
    /// <summary>
    /// Gets the unique identifier of the user whose response was removed.
    /// </summary>
    public Snowflake User { get; init; }
}
