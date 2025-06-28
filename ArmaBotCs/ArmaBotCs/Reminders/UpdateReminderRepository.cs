using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Reminders;

public sealed class UpdateReminderRepository(IAggregateRepository<Guid, Mission> aggregateRepository, IUpdateReminderBackgroundTask updateReminderBackgroundTask) : IAggregateRepository<Guid, Mission>
{
    public async Task<Mission> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await aggregateRepository.LoadAsync(id, cancellationToken);
    }

    public async Task SaveAsync(Mission aggregate, CancellationToken cancellationToken)
    {
        await aggregateRepository.SaveAsync(aggregate, cancellationToken);
        var reminder = new Reminder()
        {
            Id = aggregate.Id,
            Date = aggregate.GetMissionData().Date,
        };
        await updateReminderBackgroundTask.UpdateReminder(reminder);
    }
}
