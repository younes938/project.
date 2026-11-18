using ETicket.Application.Identity;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Application.Ports;

public interface IAuthAdapter
{
    Task<User?> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken);

    Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IVmsAdapter
{
    Task<IReadOnlyCollection<CameraRecognition>> ListRecognitionsAsync(
        GateMode mode,
        CancellationToken cancellationToken);

    Task MarkProcessedAsync(string recognitionId, CancellationToken cancellationToken);
}

public interface IEQueueAdapter
{
    Task<QueueEntry?> FindByPlateAsync(
        Guid checkpointId,
        string plateNumber,
        CancellationToken cancellationToken);

    Task SetPausedAsync(bool paused, CancellationToken cancellationToken);
}

public interface ICustomsAdapter
{
    Task<ControlResult?> GetControlResultAsync(
        Guid vehicleVisitId,
        CancellationToken cancellationToken);
}

public interface IBorderControlAdapter
{
    Task<ControlResult?> GetControlResultAsync(
        Guid vehicleVisitId,
        CancellationToken cancellationToken);
}
