using Remora.Rest.Core;

namespace ArmaBot.Core.Models;

/// <summary>
/// Represents a user's response to a mission, including RSVP status, side, and selected roles.
/// </summary>
public sealed record Response
{
    /// <summary>
    /// Gets the unique identifier of the user who submitted the response.
    /// </summary>
    public Snowflake User { get; init; }

    /// <summary>
    /// Gets the RSVP status indicating the user's intent to participate in the mission.
    /// </summary>
    public Enums.Rsvp Rsvp { get; init; }

    /// <summary>
    /// Gets the side or faction the user is associated with for the mission.
    /// </summary>
    public Enums.Side Side { get; init; }

    /// <summary>
    /// Gets the user's primary role selection for the mission.
    /// </summary>
    public Enums.Role PrimaryRole { get; init; }

    /// <summary>
    /// Gets the user's secondary role selection for the mission, if any.
    /// </summary>
    public Enums.Role SecondaryRole { get; init; }

    /// <summary>
    /// Gets the user's tertiary role selection for the mission, if any.
    /// </summary>
    public Enums.Role TertiaryRole { get; init; }
}
