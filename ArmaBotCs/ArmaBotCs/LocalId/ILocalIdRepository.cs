using System;
using System.Threading.Tasks;

namespace ArmaBotCs.LocalId;

internal interface ILocalIdRepository
{
    public Task<string> GetOrAddLocalIdAsync(Guid missionId);

    public Guid? GetMissionIdByLocalId(string localId);
}
