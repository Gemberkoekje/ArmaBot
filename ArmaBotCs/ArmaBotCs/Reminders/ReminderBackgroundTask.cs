using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qowaiv;
using Remora.Discord.API.Abstractions.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Reminders;

/// <summary>
/// Background service that manages and triggers reminders for missions or events in Discord guilds.
/// Handles scheduling, updating, and sending reminder notifications.
/// </summary>
public sealed class ReminderBackgroundTask : BackgroundService, IUpdateReminderBackgroundTask
{
    /// <summary>
    /// Gets the in-memory list of scheduled reminders.
    /// </summary>
    private readonly List<Reminder> Reminders;

    /// <summary>
    /// Provides access to application services for dependency resolution.
    /// </summary>
    private readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReminderBackgroundTask"/> class and loads existing reminders from the database.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies and database sessions.</param>
    public ReminderBackgroundTask(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        using var session = serviceProvider.GetService<IDocumentStore>().QuerySession();
        Reminders = session.Query<Reminder>().ToList();
    }

    /// <summary>
    /// Updates the specified reminder in the database and in-memory list.
    /// If the reminder exists, its date is updated; otherwise, it is added.
    /// </summary>
    /// <param name="reminder">The <see cref="Reminder"/> to update or add.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateReminderAsync(Reminder reminder)
    {
        if (reminder.Date.AddMinutes(-31) < Clock.UtcNow())
        {
            // Don't save a reminder in the past
            return;
        }
        using var scope = ServiceProvider.CreateScope();
        using var session = ServiceProvider.GetService<IDocumentStore>().LightweightSession();
        session.Store(reminder);
        await session.SaveChangesAsync();
        var savedReminder = Reminders.FirstOrDefault(r => r.Id == reminder.Id);
        if (savedReminder != null)
        {
            Reminders.Remove(savedReminder);
            savedReminder = savedReminder with { Date = reminder.Date };
            Reminders.Add(savedReminder);
        }
        else
        {
            Reminders.Add(reminder);
        }
    }

    /// <summary>
    /// Executes the background task, periodically checking for due reminders and sending notifications.
    /// Removes reminders after they are triggered.
    /// </summary>
    /// <param name="stoppingToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the background execution loop.</returns>
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = Clock.UtcNow();
            var dueReminders = Reminders
                .Where(r => r.Date.AddMinutes(-31) <= now)
                .ToList();

            foreach (var reminder in dueReminders)
            {
                await SendDiscordMessageAsync(reminder, stoppingToken);
                Reminders.Remove(reminder);
                using var scope = ServiceProvider.CreateScope();
                using var session = scope.ServiceProvider.GetService<IDocumentStore>().LightweightSession();
                session.Delete(reminder);
                await session.SaveChangesAsync(stoppingToken);
            }

            // Wait for a short interval before checking again
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    /// <summary>
    /// Sends a reminder notification message to the appropriate Discord channel for the specified reminder.
    /// </summary>
    /// <param name="reminder">The <see cref="Reminder"/> to notify about.</param>
    /// <param name="token">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous send operation.</returns>
    private async Task SendDiscordMessageAsync(Reminder reminder, CancellationToken token)
    {
        using var scope = ServiceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetService<IAggregateRepository<Guid, Mission>>();
        var channelApi = scope.ServiceProvider.GetService<IDiscordRestChannelAPI>();
        var mission = await repository.LoadAsync(reminder.Id, token);
        var unixTimestamp = new DateTimeOffset(mission.GetMissionData().Date).ToUnixTimeSeconds();
        var result = await channelApi.CreateMessageAsync(
            mission.GetMissionData().Channel, // This should be a Snowflake
            $"<@&{mission.GetMissionData().RoleToPing}>⏰ Reminder: {mission.GetMissionData().Op} will start on <t:{unixTimestamp}:F> (<t:{unixTimestamp}:R>)",
            ct: token);

        if (!result.IsSuccess)
        {
            // Handle error (logging, retry, etc.)
            Console.WriteLine($"Failed to send reminder: {result.Error.Message}");
        }
        else
        {
            Console.WriteLine($"Reminder triggered: {reminder.Id} at {reminder.Date}");
        }
    }
}
