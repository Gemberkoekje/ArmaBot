using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

public interface IEventStore
{
    Task SaveEventsAsync(Guid aggregateId, IEnumerable<object> events, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<object>> LoadEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
}