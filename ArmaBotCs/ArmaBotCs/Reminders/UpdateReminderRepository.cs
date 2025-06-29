using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Reminders;

/// <summary>
/// Decorates an <see cref="IAggregateRepository{Guid, Mission}"/> to automatically update or schedule reminders
/// whenever a mission aggregate is saved.
/// </summary>
public sealed class UpdateReminderRepository(
    IAggregateRepository<Guid, Mission> aggregateRepository,
    IUpdateReminderBackgroundTask updateReminderBackgroundTask
) : IAggregateRepository<Guid, Mission>
{
    /// <summary>
    /// Asynchronously loads a mission aggregate by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the mission aggregate to load.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>
    /// A task that represents the asynchronous load operation. The task result contains the loaded <see cref="Mission"/> aggregate.
    /// </returns>
    public async Task<Mission> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await aggregateRepository.LoadAsync(id, cancellationToken);
    }

    /// <summary>
    /// Asynchronously saves the specified mission aggregate and updates or schedules a corresponding reminder.
    /// </summary>
    /// <param name="aggregate">The <see cref="Mission"/> aggregate to save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous save and reminder update operation.</returns>
    public async Task SaveAsync(Mission aggregate, CancellationToken cancellationToken)
    {
        await aggregateRepository.SaveAsync(aggregate, cancellationToken);
        var reminder = new Reminder()
        {
            Id = aggregate.Id,
            Date = aggregate.GetMissionData().Date,
            Guild = aggregate.GetMissionData().Guild,
        };
        await updateReminderBackgroundTask.UpdateReminderAsync(reminder);
    }
}
