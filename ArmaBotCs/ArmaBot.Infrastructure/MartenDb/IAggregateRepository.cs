using System.Threading;
using System.Threading.Tasks;

namespace ArmaBot.Infrastructure.MartenDb;

public interface IAggregateRepository<TId, TValue> where TValue : class
{
    public Task SaveAsync(TValue aggregate, CancellationToken cancellationToken);

    public Task<TValue> LoadAsync(TId id, CancellationToken cancellationToken = default);
}
