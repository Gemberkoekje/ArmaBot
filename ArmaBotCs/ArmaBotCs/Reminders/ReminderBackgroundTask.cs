using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.MartenDb;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Abstractions.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs.Reminders;

public class ReminderBackgroundTask : BackgroundService, IUpdateReminderBackgroundTask
{
    public List<Reminder> Reminders;
    public IServiceProvider _serviceProvider;

    public ReminderBackgroundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        using var session = serviceProvider.GetService<IDocumentStore>().QuerySession();
        Reminders = session.Query<Reminder>().ToList();
    }


    public async Task UpdateReminder(Reminder reminder)
    {
        using var scope = _serviceProvider.CreateScope();
        using var session = _serviceProvider.GetService<IDocumentStore>().LightweightSession();
        session.Store(reminder);
        await session.SaveChangesAsync();
        var savedReminder = Reminders.FirstOrDefault(r => r.Id == reminder.Id);
        if (savedReminder != null)
        {
            Reminders.Remove(savedReminder);
            savedReminder = savedReminder with { Date = reminder.Date};
            Reminders.Add(savedReminder);
        }
        else
        {
            Reminders.Add(reminder);
        }
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var dueReminders = Reminders
                .Where(r => r.Date.AddMinutes(-30) <= now)
                .ToList();

            foreach (var reminder in dueReminders)
            {
                await SendDiscordMessageAsync(reminder, stoppingToken);
                Reminders.Remove(reminder);
                using var scope = _serviceProvider.CreateScope();
                using var session = scope.ServiceProvider.GetService<IDocumentStore>().LightweightSession();
                session.Delete(reminder);
                await session.SaveChangesAsync(stoppingToken);
            }

            // Wait for a short interval before checking again
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }

    private async Task SendDiscordMessageAsync(Reminder reminder, CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetService<IAggregateRepository<Guid, Mission>>();
        var channelApi = scope.ServiceProvider.GetService<IDiscordRestChannelAPI>();
        var mission = await repository.LoadAsync(reminder.Id, token);
        var unixTimestamp = new DateTimeOffset(mission.GetMissionData().Date).ToUnixTimeSeconds();
        var result = await channelApi.CreateMessageAsync(
            mission.GetMissionData().Channel, // This should be a Snowflake
            $"<@&{mission.GetMissionData().RoleToPing}>⏰ Reminder: {mission.GetMissionData().Op} will start on <t:{unixTimestamp}:F> (<t:{unixTimestamp}:R>)",
            ct: token
        );

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
