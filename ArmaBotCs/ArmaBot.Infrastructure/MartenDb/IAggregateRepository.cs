using ArmaBot.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

public interface IAggregateRepository<T> where T : class
{
    public Task SaveAsync(T aggregate, CancellationToken cancellationToken);

    public Task<T> LoadAsync(Guid id, CancellationToken cancellationToken = default);
}
