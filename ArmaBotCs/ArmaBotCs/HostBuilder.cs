using ArmaBot.Core.Models;
using ArmaBot.Infrastructure.Converters;
using ArmaBot.Infrastructure.MartenDb;
using ArmaBot.Infrastructure.Postgress;
using ArmaBot.Infrastructure.Postgress.Podclaim;
using ArmaBotCs.Commands;
using ArmaBotCs.LocalId;
using ArmaBotCs.Permissions;
using ArmaBotCs.Posts;
using ArmaBotCs.Reminders;
using Marten;
using Marten.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaBotCs;

internal static class MyHostBuilder
{
    /// <summary>
    /// Creates a generic application host builder.
    /// </summary>
    /// <param name="args">The arguments passed to the application.</param>
    /// <param name="podId">The ID of the pod running.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder CreateHostBuilder(string[] args, string podId) => Host.CreateDefaultBuilder(args)
       .ConfigureLogging(logging => logging.AddConsole())
    .AddDiscordService(services =>
    {
        var configuration = services.GetRequiredService<IConfiguration>();

        return configuration.GetValue<string>("Discord:Token") ??
               throw new InvalidOperationException(
                   "No bot token has been provided. Set the Discord:Token environment variable to a " +
                   "valid token.");
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.Configure<DiscordGatewayClientOptions>(g => g.Intents |= GatewayIntents.MessageContents);
        services.AddSingleton(new PodIdProvider(podId));

        // Discover all CommandGroup types in the current assembly
        var commandGroupTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(CommandGroup).IsAssignableFrom(t))
            .ToList();

        var commandTreeBuilder = services.AddDiscordCommands(true).AddCommandTree();

        foreach (var type in commandGroupTypes)
        {
            // Find the correct generic method with no parameters
            var method = commandTreeBuilder.GetType()
                .GetMethods()
                .First(m => m.Name == "WithCommandGroup" && m.GetGenericArguments().Length == 1 && m.GetParameters().Length == 0);
            var generic = method.MakeGenericMethod(type);
            generic.Invoke(commandTreeBuilder, null);
        }
        services.AddPostgressServices(configuration);
        services.AddHostedService<Worker>();
        services.AddMarten(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("Postgres");
            var options = new StoreOptions();
            options.Connection(connectionString);

            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            jsonOptions.Converters.Add(new SnowflakeJsonConverter());

            options.Serializer(new SystemTextJsonSerializer(jsonOptions));
            return options;
        });
        services.AddSingleton<IEventStore<Guid>, MartenEventStore>();
        services.AddScoped(typeof(MissionRepository));
        services.AddScoped<IAggregateRepository<Guid, Mission>>(a => a.GetRequiredService<MissionRepository>());
        services.AddResponder<ReadyResponder>();
        services.AddSingleton<ReminderBackgroundTask>();
        services.AddHostedService(sp => sp.GetRequiredService<ReminderBackgroundTask>());
        services.AddSingleton<IUpdateReminderBackgroundTask>(sp => sp.GetRequiredService<ReminderBackgroundTask>());
        services.AddSingleton<PostUpdater>();
        services.AddSingleton<ILocalIdRepository, LocalIdRepository>();
        services.AddSingleton<IPostRepository>(sp => sp.GetRequiredService<PostUpdater>());
        services.AddSingleton(sp =>
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.General);
            options.Converters.Add(new SnowflakeJsonConverter());
            options.Converters.Add(new JsonStringEnumConverter());
            options.WriteIndented = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            return options;
        });
        services.AddScoped<ICheckPermissions, CheckPermissions>();
    });
}
