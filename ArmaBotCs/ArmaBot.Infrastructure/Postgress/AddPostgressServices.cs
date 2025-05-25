using ArmaBot.Infrastructure.Postgress.Podclaim;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaBot.Infrastructure.Postgress;

public static class PostgressServiceCollectionExtensions
{
    /// <summary>
    /// Adds Marten/PostgreSQL and related services to the DI container.
    /// </summary>
    public static IServiceCollection AddPostgressServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        services.AddMarten(connectionString);
        services.AddSingleton<IPodClaimService, PodClaimService>();
        return services;
    }
}