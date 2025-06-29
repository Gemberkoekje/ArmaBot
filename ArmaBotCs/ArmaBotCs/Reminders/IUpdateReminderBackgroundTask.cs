using System.Threading.Tasks;

namespace ArmaBotCs.Reminders;

/// <summary>
/// Defines a contract for a background task that updates reminders for missions or events.
/// </summary>
public interface IUpdateReminderBackgroundTask
{
    /// <summary>
    /// Updates the specified reminder, typically scheduling or rescheduling a background operation.
    /// </summary>
    /// <param name="reminder">
    /// The <see cref="Reminder"/> object containing the guild, unique identifier, and scheduled date for the reminder.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous update operation.
    /// </returns>
    Task UpdateReminderAsync(Reminder reminder);
}
