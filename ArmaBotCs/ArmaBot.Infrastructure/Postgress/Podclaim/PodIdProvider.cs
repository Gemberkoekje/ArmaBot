namespace ArmaBot.Infrastructure.Postgress.Podclaim;

/// <summary>
/// Provides the identifier for a specific pod instance.
/// </summary>
public sealed class PodIdProvider(string podId)
{
    /// <summary>
    /// Gets the unique identifier of the pod.
    /// </summary>
    public string PodId { get; } = podId;
}
