using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.Postgress.Podclaim;

/// <summary>
/// Provides methods for claiming and releasing pods using a Discord token as an identifier.
/// </summary>
public interface IPodClaimService
{
    /// <summary>
    /// Attempts to claim a pod for the specified Discord user.
    /// </summary>
    /// <param name="discordToken">The Discord token identifying the user attempting to claim the pod.</param>
    /// <param name="podId">The unique identifier of the pod to claim.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains <c>true</c> if the pod was successfully claimed; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> TryClaimPodAsync(string discordToken, string podId, CancellationToken cancellationToken);

    /// <summary>
    /// Releases a previously claimed pod for the specified Discord user.
    /// </summary>
    /// <param name="discordToken">The Discord token identifying the user releasing the pod.</param>
    /// <param name="podId">The unique identifier of the pod to release.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ReleasePodAsync(string discordToken, string podId, CancellationToken cancellationToken);
}
