using Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

/// <summary>
/// Implements the <see cref="IEventStore{Guid}"/> interface using Marten to persist and retrieve domain events for aggregates.
/// </summary>
public sealed class MartenEventStore : IEventStore<Guid>
{
    private readonly IDocumentStore _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="MartenEventStore"/> class with the specified Marten document store.
    /// </summary>
    /// <param name="store">The Marten <see cref="IDocumentStore"/> used for event persistence.</param>
    public MartenEventStore(IDocumentStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Asynchronously saves a collection of events for the specified aggregate using a lightweight Marten session.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate to which the events belong.</param>
    /// <param name="events">The collection of events to persist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        using var session = _store.LightweightSession();
        session.Events.Append(aggregateId, events);
        await session.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads all events associated with the specified aggregate using a Marten query session.
    /// </summary>
    /// <param name="aggregateId">The unique identifier of the aggregate whose events are to be loaded.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>
    /// A task that represents the asynchronous load operation. The task result contains a read-only list of events for the aggregate.
    /// </returns>
    public async Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        using var session = _store.QuerySession();
        var stream = await session.Events.FetchStreamAsync(aggregateId, token: cancellationToken);
        return stream.Select(e => e.Data).ToList();
    }
}
