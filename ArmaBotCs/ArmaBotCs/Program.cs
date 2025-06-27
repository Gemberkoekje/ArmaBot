using ArmaBot.Infrastructure.Postgress;
using ArmaBot.Infrastructure.Postgress.Podclaim;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

internal sealed class Program
{
    private Program() { }

    public static async Task Main(string[] args)
    {
#if DEBUG
        const string podId = "debug-pod"; // Use a fixed pod ID in debug mode for easier testing
#else
        var podId = Guid.NewGuid().ToString();
#endif
        var cancellationToken = CancellationToken.None;

        while (true)
        {
            Console.WriteLine("Starting...");
            // Build a minimal service provider to access configuration and pod claim service
            using var configHost = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddPostgressServices(context.Configuration);
                })
                .Build();

            var config = configHost.Services.GetRequiredService<IConfiguration>();
            var discordToken = config.GetValue<string>("Discord:Token");

            var podClaimService = configHost.Services.GetRequiredService<IPodClaimService>();
            if (await podClaimService.TryClaimPodAsync(discordToken, podId, cancellationToken))
            {
                // Pod claimed, now build and run the full host (with Discord)
                var host = MyHostBuilder.CreateHostBuilder(args, podId).Build();
                await host.RunAsync();
                break;
            }
            else
            {
                // Pod not claimed, wait and retry
                Console.WriteLine("Pod claim failed. Retrying in 10 minutes...");
                await Task.Delay(TimeSpan.FromMinutes(10), cancellationToken);
            }
        }
    }
}
