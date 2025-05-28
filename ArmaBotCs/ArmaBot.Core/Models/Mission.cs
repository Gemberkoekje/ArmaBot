using ArmaBot.Core.Identifiers;
using ArmaBot.Core.Validators;
using Qowaiv.DomainModel;

namespace ArmaBot.Core.Models;

internal partial class Mission : Aggregate<Mission, MissionId>
{
    public Mission() : this(MissionId.Next())
    {
    }

    protected Mission(MissionId aggregateId) : base(aggregateId, new MissionValidator())
    {
    }
}
