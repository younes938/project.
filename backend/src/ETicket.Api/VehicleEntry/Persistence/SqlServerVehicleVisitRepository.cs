using System.Text.Json;
using ETicket.Application.Ports;
using ETicket.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ETicket.Api.VehicleEntry.Persistence;

public sealed class SqlServerVehicleVisitRepository(VehicleVisitDbContext db) : IVehicleVisitRepository
{
    public async Task<StoredVehicleVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.VehicleVisits.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return entity is null ? null : ToStored(entity);
    }

    public async Task<IReadOnlyCollection<StoredVehicleVisit>> ListByCheckpointAsync(
        Guid checkpointId, CancellationToken cancellationToken)
    {
        var entities = await db.VehicleVisits.AsNoTracking()
            .Where(item => item.CheckpointId == checkpointId)
            .OrderByDescending(item => item.EnteredAt)
            .ToListAsync(cancellationToken);
        return entities.Select(ToStored).ToArray();
    }

    public async Task<AddVehicleVisitResult> AddOrGetExistingAsync(
        StoredVehicleVisit visit, CancellationToken cancellationToken)
    {
        var entity = ToEntity(visit);
        db.VehicleVisits.Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return new AddVehicleVisitResult(true, visit);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            var existing = await db.VehicleVisits.AsNoTracking().SingleOrDefaultAsync(
                item => item.CreatedByUserId == visit.Visit.CreatedByUserId &&
                        item.IdempotencyKey == visit.IdempotencyKey, cancellationToken);
            if (existing is null) throw;
            return new AddVehicleVisitResult(false, ToStored(existing));
        }
    }

    public async Task<bool> TryUpdateAsync(
        VehicleVisit visit, long expectedVersion, CancellationToken cancellationToken)
    {
        var nextVersion = expectedVersion + 1;
        var updatedVisit = visit with { Version = nextVersion };
        var json = JsonSerializer.Serialize(updatedVisit);
        var changed = await db.VehicleVisits
            .Where(item => item.Id == visit.Id && item.Version == expectedVersion)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.VisitJson, json)
                .SetProperty(item => item.Version, nextVersion), cancellationToken);
        return changed == 1;
    }

    private static VehicleVisitEntity ToEntity(StoredVehicleVisit stored) => new()
    {
        Id = stored.Visit.Id,
        VehicleId = stored.Vehicle.Id,
        CreatedByUserId = stored.Visit.CreatedByUserId,
        CheckpointId = stored.Visit.CheckpointId,
        EnteredAt = stored.Visit.EnteredAt,
        Version = stored.Visit.Version,
        IdempotencyKey = stored.IdempotencyKey,
        RequestFingerprint = stored.RequestFingerprint,
        VehicleJson = JsonSerializer.Serialize(stored.Vehicle),
        VisitJson = JsonSerializer.Serialize(stored.Visit),
        QueueEntryJson = stored.QueueEntry is null ? null : JsonSerializer.Serialize(stored.QueueEntry),
    };

    private static StoredVehicleVisit ToStored(VehicleVisitEntity entity) => new(
        JsonSerializer.Deserialize<Vehicle>(entity.VehicleJson)
            ?? throw new InvalidOperationException("Stored vehicle JSON was empty."),
        JsonSerializer.Deserialize<VehicleVisit>(entity.VisitJson)
            ?? throw new InvalidOperationException("Stored vehicle visit JSON was empty."),
        entity.QueueEntryJson is null ? null : JsonSerializer.Deserialize<QueueEntry>(entity.QueueEntryJson),
        entity.IdempotencyKey,
        entity.RequestFingerprint);
}
