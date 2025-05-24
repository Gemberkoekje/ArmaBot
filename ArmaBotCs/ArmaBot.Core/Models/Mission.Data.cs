using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;

namespace ArmaBot.Core.Models;

public partial class Mission
{
    public MissionData MissionData { get; private set; }

    public Result<Mission> SetData(MissionData missionData)
    => ApplyEvent(new MissionDataSet
        {
            Campaign = missionData.Campaign,
            Modset = missionData.Modset,
            Op = missionData.Op,
            Date = missionData.Date,
            Description = missionData.Description,
            Channel = missionData.Channel,
            UserToPing = missionData.UserToPing,
            Sides = missionData.Sides,
        });

    internal void When(MissionDataSet @event)
    {
        MissionData = new MissionData
        {
            Campaign = @event.Campaign,
            Modset = @event.Modset,
            Op = @event.Op,
            Date = @event.Date,
            Description = @event.Description,
            Channel = @event.Channel,
            UserToPing = @event.UserToPing,
            Sides = @event.Sides,
        };
    }
}
