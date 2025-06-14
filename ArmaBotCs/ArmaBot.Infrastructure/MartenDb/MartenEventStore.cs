using global::Marten;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

public class MartenEventStore : IEventStore
{
    private readonly IDocumentStore _store;

    public MartenEventStore(IDocumentStore store)
    {
        _store = store;
    }

    public async Task SaveEventsAsync(Guid aggregateId, IEnumerable<object> events, CancellationToken cancellationToken = default)
    {
        using var session = _store.LightweightSession();
        session.Events.Append(aggregateId, events);
        await session.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        using var session = _store.QuerySession();
        var stream = await session.Events.FetchStreamAsync(aggregateId, token: cancellationToken);
        return stream.Select(e => e.Data).ToList();
    }
}