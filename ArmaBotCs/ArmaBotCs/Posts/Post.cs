using Remora.Rest.Core;
using System;

namespace ArmaBotCs.Posts;

/// <summary>
/// Represents a mission post within a Discord guild, including identifiers and scheduling information.
/// </summary>
public sealed record Post
{
    /// <summary>
    /// Gets the unique identifier of the Discord guild where the mission post is associated.
    /// </summary>
    required public Snowflake Guild { get; init; }

    /// <summary>
    /// Gets the unique identifier of the mission.
    /// </summary>
    required public Guid Id { get; init; }

    /// <summary>
    /// Gets the operation name or title of the mission.
    /// </summary>
    required public string Op { get; init; }

    /// <summary>
    /// Gets the unique identifier of the Discord post message, or <c>null</c> if not yet posted.
    /// </summary>
    required public Snowflake? PostId { get; init; }

    /// <summary>
    /// Gets the scheduled date and time of the mission.
    /// </summary>
    required public DateTime MissionDate { get; init; }
}
