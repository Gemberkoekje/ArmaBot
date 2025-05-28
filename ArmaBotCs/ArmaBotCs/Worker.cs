using ArmaBot.Infrastructure.Postgress.Podclaim;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

internal sealed class Worker(ILogger<Worker> logger, IPodClaimService podClaimService, IConfiguration config, PodIdProvider podIdProvider) : BackgroundService
{
    private readonly string _discordToken = config.GetValue<string>("Discord:Token");
    private readonly string _podId = podIdProvider.PodId;
    private readonly TimeSpan _claimTimeout = TimeSpan.FromMinutes(1); // Should match PodClaimService

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var claimed = await podClaimService.TryClaimPodAsync(_discordToken, _podId, stoppingToken);
            if (!claimed)
            {
                logger.LogWarning("Pod claim failed. Retrying in {Timeout}...", _claimTimeout);
                await Task.Delay(_claimTimeout, stoppingToken);
                continue;
            }

            logger.LogInformation("Pod claim successful. Entering keep-alive loop.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Refresh the claim to keep it alive
                    await podClaimService.TryClaimPodAsync(_discordToken, _podId, stoppingToken);
                    await Task.Delay(_claimTimeout, stoppingToken);
                }
            }
            finally
            {
                await podClaimService.ReleasePodAsync(_discordToken, _podId, CancellationToken.None);
                logger.LogInformation("Pod claim released.");
            }
        }
    }
}
