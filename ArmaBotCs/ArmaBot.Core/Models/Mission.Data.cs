using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;

namespace ArmaBot.Core.Models;

/// <summary>
/// Provides mission data management for the <see cref="Mission"/> aggregate, including setting and applying mission details.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// Gets the data describing this mission, such as campaign, modset, operation, schedule, and Discord context.
    /// </summary>
    internal MissionData MissionData { get; private set; }

    /// <summary>
    /// Sets the mission data for this mission aggregate by applying a <see cref="MissionDataSet"/> event.
    /// </summary>
    /// <param name="missionData">The new mission data to set.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation results.
    /// </returns>
    public Result<Mission> SetData(MissionData missionData)
    => ApplyEvent(new MissionDataSet
    {
        Campaign = missionData.Campaign,
        Modset = missionData.Modset,
        Op = missionData.Op,
        Date = missionData.Date,
        Description = missionData.Description,
        Channel = missionData.Channel,
        Guild = missionData.Guild,
        RoleToPing = missionData.RoleToPing,
    });

    /// <summary>
    /// Handles the <see cref="MissionDataSet"/> event by updating the <see cref="MissionData"/> property.
    /// </summary>
    /// <param name="event">The event containing the new mission data.</param>
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
            Guild = @event.Guild,
            RoleToPing = @event.RoleToPing,
        };
    }
}
