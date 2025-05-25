namespace ArmaBot.Infrastructure.Postgress.Podclaim;

public class PodIdProvider
{
    public string PodId { get; }

    public PodIdProvider(string podId) => PodId = podId;
}
