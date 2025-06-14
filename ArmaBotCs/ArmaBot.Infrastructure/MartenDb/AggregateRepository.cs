using ArmaBot.Core.Identifiers;
using ArmaBot.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

public class MissionRepository
{
    private readonly IEventStore _eventStore;

    public MissionRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task SaveAsync(Mission aggregate, CancellationToken cancellationToken = default)
    {
        // Assuming aggregate exposes a list of uncommitted events (e.g., Events)
        await _eventStore.SaveEventsAsync(Guid.Parse(aggregate.Id.ToString()), aggregate.Buffer, cancellationToken);
    }

    public async Task<Mission> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var events = await _eventStore.LoadEventsAsync(id, cancellationToken);
        var aggregate = new Mission(MissionId.Parse(id.ToString()));
        aggregate.ApplyEvents(events);
        return aggregate;
    }
}