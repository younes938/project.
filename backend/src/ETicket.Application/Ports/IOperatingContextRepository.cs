using ETicket.Domain.Models;

namespace ETicket.Application.Ports;

public interface IOperatingContextRepository
{
    Task<OperatingContext?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task UpsertAsync(OperatingContext operatingContext, CancellationToken cancellationToken);

    Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

public interface ICheckpointCatalog
{
    Task<IReadOnlyCollection<Checkpoint>> ListCheckpointsAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Gate>> ListGatesAsync(CancellationToken cancellationToken);

    Task<Checkpoint?> GetCheckpointAsync(
        Guid checkpointId,
        CancellationToken cancellationToken);

    Task<Gate?> GetGateAsync(Guid gateId, CancellationToken cancellationToken);
}
