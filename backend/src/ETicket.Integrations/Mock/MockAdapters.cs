using ETicket.Application.Identity;
using ETicket.Application.Ports;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Integrations.Mock;

public sealed class MockAuthAdapter : IAuthAdapter
{
    private const string DemoSubject = "phase2-demo-operator";

    private static readonly Guid DemoUserId = Guid.Parse("8fd9706f-a25e-4dbd-85cc-14d61a93106d");
    private static readonly Guid DemoRoleId = Guid.Parse("8cc3426a-aec7-4e39-839a-e21467ac9515");
    private static readonly Guid DemoZoneId = Guid.Parse("43470890-c54d-49cc-bd9b-b09d535d5a56");
    private static readonly Guid DemoCheckpointId = Guid.Parse("19fb5569-2fcf-4d4e-9535-2d2798702af8");

    private static readonly User DemoUser = new(
        DemoUserId,
        DemoSubject,
        "Demo Border Control Operator",
        [
            new Role(
                DemoRoleId,
                "checkpoint-operator",
                "Checkpoint Operator",
                new HashSet<Permission>
                {
                    Permission.ProcessVehicle,
                    Permission.InspectVehicle,
                    Permission.SearchRecentVehicles,
                    Permission.ViewVehicle,
                    Permission.ViewQueue
                })
        ],
        [
            new VisibilityZone(
                DemoZoneId,
                "Demo Border Crossing",
                new HashSet<Guid> { DemoCheckpointId })
        ],
        true);

    public Task<User?> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = request is { Method: AuthenticationMethod.Mock, Subject: DemoSubject }
            ? DemoUser
            : null;

        return Task.FromResult(user);
    }

    public Task<User?> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(userId == DemoUserId ? DemoUser : null);
    }
}

public sealed class MockVmsAdapter : IVmsAdapter
{
    public Task<IReadOnlyCollection<CameraRecognition>> ListRecognitionsAsync(
        GateMode mode,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyCollection<CameraRecognition>>([]);
    }

    public Task MarkProcessedAsync(string recognitionId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

public sealed class MockEQueueAdapter : IEQueueAdapter
{
    private static readonly Guid DemoCheckpointId =
        Guid.Parse("19fb5569-2fcf-4d4e-9535-2d2798702af8");

    public Task<QueueEntry?> FindByPlateAsync(
        Guid checkpointId,
        string plateNumber,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entry = checkpointId == DemoCheckpointId && plateNumber == "AA1234BB"
            ? new QueueEntry(
                "eq-demo-001",
                checkpointId,
                plateNumber,
                "confirmed",
                DateTimeOffset.Parse("2026-09-07T08:00:00Z"))
            : null;

        return Task.FromResult(entry);
    }

    public Task SetPausedAsync(bool paused, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}

public sealed class MockCustomsAdapter : ICustomsAdapter
{
    public Task<ControlResult?> GetControlResultAsync(
        Guid vehicleVisitId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<ControlResult?>(null);
    }
}

public sealed class MockBorderControlAdapter : IBorderControlAdapter
{
    public Task<ControlResult?> GetControlResultAsync(
        Guid vehicleVisitId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<ControlResult?>(null);
    }
}
