using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

/// <summary>
/// Defines a contract for an event store that persists and retrieves domain events for aggregates.
/// </summary>
/// <typeparam name="TId">The type of the aggregate's unique identifier.</typeparam>
public interface IEventStore<TId>
{
    /// <summary>
    /// Asynchronously saves a collection of events for the specified aggregate.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate to which the events belong.</param>
    /// <param name="events">The collection of events to persist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveEventsAsync(TId aggregateId, IEnumerable<object> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously loads all events associated with the specified aggregate.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate whose events are to be loaded.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>
    /// A task that represents the asynchronous load operation. The task result contains a read-only list of events for the aggregate.
    /// </returns>
    Task<IReadOnlyList<object>> LoadEventsAsync(TId aggregateId, CancellationToken cancellationToken = default);
}
