using System.Threading.Tasks;

namespace ArmaBotCs.Reminders;

public interface IUpdateReminderBackgroundTask
{
    public Task UpdateReminder(Reminder reminder);
}
