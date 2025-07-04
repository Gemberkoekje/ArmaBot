using ArmaBot.Core.Events;
using Qowaiv.Validation.Abstractions;
using System;

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
    /// Retrieves the current mission data for this mission aggregate.
    /// </summary>
    /// <returns>
    /// The <see cref="MissionData"/> describing this mission.
    /// </returns>
    public MissionData GetMissionData()
    {
        return MissionData;
    }

    /// <summary>
    /// Sets the mission data for this mission aggregate by applying a <see cref="MissionDataSet"/> event.
    /// </summary>
    /// <param name="missionData">The new mission data to set.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating the outcome of the operation, including validation results.
    /// </returns>
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
    public Result<Mission> SetData(UpdateMissionData missionData)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
    {
        if (MissionData == null)
        {
            if (missionData.Campaign == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("Campaign must be set."));
            }
            if (missionData.Modset == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("Modset must be set."));
            }
            if (missionData.Op == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("Op must be set."));
            }
            if (missionData.Date == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("Date must be set."));
            }
            if (missionData.Channel == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("Channel must be set."));
            }
            if (missionData.Guild == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("Guild must be set."));
            }
            if (missionData.RoleToPing == null)
            {
                return Result.WithMessages<Mission>(ValidationMessage.Error("RoleToPing must be set."));
            }
        }

        return ApplyEvent(new MissionDataSet
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
    }

    /// <summary>
    /// Handles the <see cref="MissionDataSet"/> event by updating the <see cref="MissionData"/> property
    /// with the values from the event.
    /// </summary>
    /// <param name="event">The event containing the new mission data.</param>
    internal void When(MissionDataSet @event)
    {
        MissionData = new MissionData
        {
            Campaign = @event.Campaign ?? MissionData.Campaign,
            Modset = @event.Modset ?? MissionData.Modset,
            Op = @event.Op ?? MissionData.Op,
            Date = @event.Date ?? MissionData.Date,
            Description = @event.Description ?? MissionData?.Description,
            Channel = @event.Channel ?? MissionData.Channel,
            Guild = @event.Guild ?? MissionData.Guild,
            RoleToPing = @event.RoleToPing ?? MissionData.RoleToPing,
        };
    }
}
