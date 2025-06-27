using ArmaBot.Core.Models;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.Interfaces;

public interface IUpdateReminderBackgroundTask
{
    public Task UpdateReminder(Reminder reminder);
}
