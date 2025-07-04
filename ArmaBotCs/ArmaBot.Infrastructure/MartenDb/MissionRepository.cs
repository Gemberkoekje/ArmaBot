#nullable enable
using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

/// <summary>
/// Implements the <see cref="IAggregateRepository{Guid, Mission}"/> interface using an event store to persist and reconstruct <see cref="Mission"/> aggregates.
/// </summary>
public sealed class MissionRepository : IAggregateRepository<Guid, Mission>
{
    private readonly IEventStore<Guid> _eventStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="MissionRepository"/> class with the specified event store.
    /// </summary>
    /// <param name="eventStore">The event store used to persist and retrieve mission events.</param>
    public MissionRepository(IEventStore<Guid> eventStore)
    {
        _eventStore = eventStore;
    }

    /// <summary>
    /// Asynchronously saves the uncommitted events of the specified <see cref="Mission"/> aggregate to the event store and marks them as committed.
    /// </summary>
    /// <param name="aggregate">The <see cref="Mission"/> aggregate whose events are to be saved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(Mission aggregate, CancellationToken cancellationToken)
    {
        // Assuming aggregate exposes a list of uncommitted events (e.g., Events)
        await _eventStore.SaveEventsAsync(aggregate.Id.ToGuid(), aggregate.Buffer.Uncommitted, cancellationToken);
        aggregate.MarkAllAsCommitted();
    }

    /// <summary>
    /// Asynchronously loads a <see cref="Mission"/> aggregate by its unique identifier by replaying its event stream.
    /// </summary>
    /// <param name="id">The unique identifier of the mission aggregate to load.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>
    /// A task that represents the asynchronous load operation. The task result contains the reconstructed <see cref="Mission"/> aggregate,
    /// or <c>null</c> if no events are found for the specified identifier.
    /// </returns>
    public async Task<Mission?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return null;

        var events = await _eventStore.LoadEventsAsync(id.ToGuid(), cancellationToken);
        if (events.Count == 0)
        {
            return null; // No events found for this aggregate ID
        }

        var aggregate = new Mission(Guid.Parse(id.ToString()));
        aggregate = aggregate.ApplyMissionEvents(events).Value;
        aggregate.MarkAllAsCommitted();
        return aggregate;
    }
}
