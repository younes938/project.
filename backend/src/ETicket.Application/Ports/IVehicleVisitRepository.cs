using ETicket.Domain.Models;

namespace ETicket.Application.Ports;

public sealed record StoredVehicleVisit(
    Vehicle Vehicle,
    VehicleVisit Visit,
    QueueEntry? QueueEntry,
    string IdempotencyKey,
    string RequestFingerprint);

public sealed record AddVehicleVisitResult(
    bool WasAdded,
    StoredVehicleVisit StoredVisit);

public interface IVehicleVisitRepository
{
    Task<StoredVehicleVisit?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StoredVehicleVisit>> ListByCheckpointAsync(
        Guid checkpointId,
        CancellationToken cancellationToken);

    Task<AddVehicleVisitResult> AddOrGetExistingAsync(
        StoredVehicleVisit visit,
        CancellationToken cancellationToken);

    Task<bool> TryUpdateAsync(
        VehicleVisit visit,
        long expectedVersion,
        CancellationToken cancellationToken);
}
