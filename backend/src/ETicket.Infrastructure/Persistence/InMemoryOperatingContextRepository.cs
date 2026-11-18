using ETicket.Application.Ports;
using ETicket.Domain.Models;

namespace ETicket.Infrastructure.Persistence;

public sealed class InMemoryOperatingContextRepository : IOperatingContextRepository
{
    private readonly Dictionary<Guid, OperatingContext> items = new();
    private readonly object sync = new();

    public Task<OperatingContext?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            return Task.FromResult(items.GetValueOrDefault(userId));
        }
    }

    public Task UpsertAsync(
        OperatingContext operatingContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            items[operatingContext.UserId] = operatingContext;
        }

        return Task.CompletedTask;
    }

    public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            items.Remove(userId);
        }

        return Task.CompletedTask;
    }
}
