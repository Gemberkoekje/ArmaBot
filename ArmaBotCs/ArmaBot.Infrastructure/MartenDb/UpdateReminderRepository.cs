using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.Interfaces;
using Marten;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

public sealed class UpdateReminderRepository(IAggregateRepository<Mission> aggregateRepository, IDocumentStore store, IUpdateReminderBackgroundTask updateReminderBackgroundTask) : IAggregateRepository<Mission>
{
    public async Task<Mission> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await aggregateRepository.LoadAsync(id, cancellationToken);
    }

    public async Task SaveAsync(Mission aggregate, CancellationToken cancellationToken)
    {
        await aggregateRepository.SaveAsync(aggregate, cancellationToken);
        using var session = store.LightweightSession();
        var reminder = new Reminder()
        {
            Id = Guid.Parse(aggregate.Id.ToString()),
            Date = aggregate.GetMissionData().Date,
        };
        session.Store(reminder);
        await session.SaveChangesAsync(cancellationToken);
        await updateReminderBackgroundTask.UpdateReminder(reminder);
    }
}
