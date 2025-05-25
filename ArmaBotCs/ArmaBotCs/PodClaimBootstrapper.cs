// ArmaBotCs/PodClaimBootstrapper.cs
using ArmaBot.Infrastructure.Postgress.Podclaim;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBotCs;

public static class PodClaimBootstrapper
{
    public static async Task<bool> TryClaimPodAsync(IServiceProvider services, string discordToken, string podId, CancellationToken cancellationToken)
    {
        var podClaimService = services.GetRequiredService<IPodClaimService>();
        return await podClaimService.TryClaimPodAsync(discordToken, podId, cancellationToken);
    }
}