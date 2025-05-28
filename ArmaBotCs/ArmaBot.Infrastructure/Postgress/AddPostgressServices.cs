using ArmaBot.Infrastructure.Postgress.Podclaim;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArmaBot.Infrastructure.Postgress;

/// <summary>
/// Provides extension methods for registering PostgreSQL and related services in the dependency injection container.
/// </summary>
public static class PostgressServiceCollectionExtensions
{
    /// <summary>
    /// Registers Marten/PostgreSQL and related services with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application configuration used to retrieve the PostgreSQL connection string.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPostgressServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        services.AddMarten(connectionString);
        services.AddSingleton<IPodClaimService, PodClaimService>();
        return services;
    }
}
