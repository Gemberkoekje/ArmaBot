using ArmaBot.Infrastructure.Postgress.Podclaim;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

internal sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IPodClaimService _podClaimService;
    private readonly string _discordToken;
    private readonly string _podId;
    private readonly TimeSpan _claimTimeout = TimeSpan.FromMinutes(1); // Should match PodClaimService

    public Worker(ILogger<Worker> logger, IPodClaimService podClaimService, IConfiguration config, PodIdProvider podIdProvider)
    {
        _logger = logger;
        _podClaimService = podClaimService;
        _discordToken = config.GetValue<string>("Discord:Token");
        _podId = podIdProvider.PodId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var claimed = await _podClaimService.TryClaimPodAsync(_discordToken, _podId, stoppingToken);
            if (!claimed)
            {
                _logger.LogWarning("Pod claim failed. Retrying in {Timeout}...", _claimTimeout);
                await Task.Delay(_claimTimeout, stoppingToken);
                continue;
            }

            _logger.LogInformation("Pod claim successful. Entering keep-alive loop.");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Refresh the claim to keep it alive
                    await _podClaimService.TryClaimPodAsync(_discordToken, _podId, stoppingToken);
                    await Task.Delay(_claimTimeout, stoppingToken);
                }
            }
            finally
            {
                await _podClaimService.ReleasePodAsync(_discordToken, _podId, CancellationToken.None);
                _logger.LogInformation("Pod claim released.");
            }
        }
    }
}
