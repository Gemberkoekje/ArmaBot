using Marten;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArmaBotCs.LocalId;

internal sealed class LocalIdRepository : ILocalIdRepository
{
    private readonly IServiceProvider _serviceProvider;

    public LocalIdRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Guid GetMissionIdByLocalId(string localId)
    {
        using var session = _serviceProvider.GetService<IDocumentStore>().QuerySession();
        return session.Query<LocalId>().FirstOrDefault(x => x.Id == localId).MissionId;
    }

    public async Task<string> GetOrAddLocalIdAsync(Guid missionId)
    {
        using var session = _serviceProvider.GetService<IDocumentStore>().LightweightSession();
        var localId = session.Query<LocalId>().FirstOrDefault(x => x.MissionId == missionId);
        if (localId == null)
        {
            var count = await session.Query<LocalId>().CountAsync();
            localId = new LocalId()
            {
                Id = $"{count + 1}",
                MissionId = missionId,
            };
            session.Store(localId);
            await session.SaveChangesAsync();
        }
        return localId.Id;
    }
}
