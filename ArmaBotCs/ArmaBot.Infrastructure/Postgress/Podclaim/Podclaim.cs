using Marten.Schema;
using System;

namespace ArmaBot.Infrastructure.Postgress.Podclaim;

/// <summary>
/// Represents a claim on a pod by a Discord user, including claim metadata and concurrency control.
/// </summary>
public sealed class PodClaim
{
    /// <summary>
    /// Gets the unique identifier for this pod claim.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the Discord token identifying the user who claimed the pod.
    /// </summary>
    public string DiscordToken { get; init; }

    /// <summary>
    /// Gets the unique identifier of the claimed pod.
    /// </summary>
    public string PodId { get; init; }

    /// <summary>
    /// Gets the UTC date and time when the pod was claimed.
    /// </summary>
    public DateTime ClaimedAt { get; init; }

    /// <summary>
    /// Gets the version identifier used for optimistic concurrency control.
    /// </summary>
    [Version]
    public Guid Version { get; init; }
}
