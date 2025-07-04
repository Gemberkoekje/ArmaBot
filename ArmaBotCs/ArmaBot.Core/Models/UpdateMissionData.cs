#nullable enable
using Remora.Rest.Core;
using System;

namespace ArmaBot.Core.Models;

/// <summary>
/// Represents the data required to define a mission, including campaign, modset, operation details, scheduling, and Discord context.
/// </summary>
public sealed class UpdateMissionData
{
    /// <summary>
    /// Gets the name of the campaign to which the mission belongs.
    /// </summary>
    required public string? Campaign { get; init; }

    /// <summary>
    /// Gets the name of the modset required for the mission.
    /// </summary>
    required public string? Modset { get; init; }

    /// <summary>
    /// Gets the name or code of the operation.
    /// </summary>
    required public string? Op { get; init; }

    /// <summary>
    /// Gets the scheduled date and time of the mission.
    /// </summary>
    required public DateTime? Date { get; init; }

    /// <summary>
    /// Gets the description of the mission.
    /// </summary>
    required public string? Description { get; init; }

    /// <summary>
    /// Gets the Discord channel where the mission will be announced or discussed.
    /// </summary>
    required public Snowflake? Channel { get; init; }

    /// <summary>
    /// Gets the Discord user to be pinged or notified about the mission.
    /// </summary>
    required public Snowflake? Guild { get; init; }

    /// <summary>
    /// Gets the Discord user to be pinged or notified about the mission.
    /// </summary>
    required public Snowflake? RoleToPing { get; init; }
}
