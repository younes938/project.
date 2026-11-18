using ETicket.Application.Ports;
using ETicket.Domain.Models;

namespace ETicket.Infrastructure.Persistence;

public sealed class InMemoryVehicleVisitRepository : IVehicleVisitRepository
{
    private readonly Dictionary<Guid, StoredVehicleVisit> items = new();
    private readonly Dictionary<(Guid UserId, string Key), Guid> idempotencyIndex = new();
    private readonly object sync = new();

    public Task<StoredVehicleVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            return Task.FromResult(items.GetValueOrDefault(id));
        }
    }

    public Task<IReadOnlyCollection<StoredVehicleVisit>> ListByCheckpointAsync(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            return Task.FromResult<IReadOnlyCollection<StoredVehicleVisit>>(
                items.Values
                    .Where(item => item.Visit.CheckpointId == checkpointId)
                    .OrderByDescending(item => item.Visit.EnteredAt)
                    .ToArray());
        }
    }

    public Task<AddVehicleVisitResult> AddOrGetExistingAsync(
        StoredVehicleVisit visit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            var key = (visit.Visit.CreatedByUserId, visit.IdempotencyKey);
            if (idempotencyIndex.TryGetValue(key, out var existingId))
            {
                return Task.FromResult(new AddVehicleVisitResult(false, items[existingId]));
            }

            if (!items.TryAdd(visit.Visit.Id, visit))
            {
                throw new InvalidOperationException($"Vehicle visit {visit.Visit.Id} already exists.");
            }

            idempotencyIndex.Add(key, visit.Visit.Id);
            return Task.FromResult(new AddVehicleVisitResult(true, visit));
        }
    }

    public Task<bool> TryUpdateAsync(
        VehicleVisit visit,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (sync)
        {
            if (!items.TryGetValue(visit.Id, out var existing) ||
                existing.Visit.Version != expectedVersion)
            {
                return Task.FromResult(false);
            }

            items[visit.Id] = existing with
            {
                Visit = visit with { Version = expectedVersion + 1 }
            };
            return Task.FromResult(true);
        }
    }
}
