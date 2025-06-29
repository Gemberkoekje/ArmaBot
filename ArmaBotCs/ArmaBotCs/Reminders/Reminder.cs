using Remora.Rest.Core;
using System;

namespace ArmaBotCs.Reminders;

/// <summary>
/// Represents a reminder for a mission or event within a Discord guild, including scheduling information.
/// </summary>
public sealed record Reminder
{
    /// <summary>
    /// Gets the unique identifier of the Discord guild associated with the reminder.
    /// </summary>
    required public Snowflake Guild { get; init; }

    /// <summary>
    /// Gets the unique identifier of the mission or event for which the reminder is set.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the scheduled date and time for the reminder.
    /// </summary>
    public DateTime Date { get; init; }
}
