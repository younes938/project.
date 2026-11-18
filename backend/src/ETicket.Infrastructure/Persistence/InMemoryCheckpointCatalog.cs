using ETicket.Application.Ports;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Infrastructure.Persistence;

public sealed class InMemoryCheckpointCatalog : ICheckpointCatalog
{
    public static readonly Guid DemoCheckpointId =
        Guid.Parse("19fb5569-2fcf-4d4e-9535-2d2798702af8");

    public static readonly Guid RearExitGateId =
        Guid.Parse("6adb7fb0-4a48-49ae-ab75-d91775205120");

    public static readonly Guid BorderExitGateId =
        Guid.Parse("f833e8e2-b8bf-43e8-a39e-8b652e429713");

    public static readonly Guid BorderEntryGateId =
        Guid.Parse("b7ef357d-7052-49f1-9219-23b991113654");

    public static readonly Guid RearEntryGateId =
        Guid.Parse("c34ee5f3-ced7-43ae-9949-e3f579fdf832");

    private static readonly Guid HiddenCheckpointId =
        Guid.Parse("656a0c4e-44ea-47fd-bf8f-f392dc61a09b");

    private static readonly IReadOnlyCollection<Checkpoint> Checkpoints =
    [
        new(
            DemoCheckpointId,
            "UA-TEST-01",
            "Demo Vehicle Border Crossing",
            true),
        new(
            HiddenCheckpointId,
            "UA-HIDDEN-02",
            "Unavailable Demo Border Crossing",
            true)
    ];

    private static readonly IReadOnlyCollection<Gate> Gates =
    [
        new(RearExitGateId, DemoCheckpointId, "REAR-EXIT", "Rear Area – Exit", GateMode.RearExit, true),
        new(BorderExitGateId, DemoCheckpointId, "BORDER-EXIT", "Border Crossing – Exit", GateMode.BorderExit, true),
        new(BorderEntryGateId, DemoCheckpointId, "BORDER-ENTRY", "Border Crossing – Entry", GateMode.BorderEntry, true),
        new(RearEntryGateId, DemoCheckpointId, "REAR-ENTRY", "Rear Area – Entry", GateMode.RearEntry, true),
        new(
            Guid.Parse("07372774-973b-4bda-be2e-b3309cfe68ca"),
            HiddenCheckpointId,
            "HIDDEN-GATE",
            "Unavailable Mode",
            GateMode.RearEntry,
            true)
    ];

    public Task<IReadOnlyCollection<Checkpoint>> ListCheckpointsAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Checkpoints);
    }

    public Task<IReadOnlyCollection<Gate>> ListGatesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Gates);
    }

    public Task<Checkpoint?> GetCheckpointAsync(
        Guid checkpointId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Checkpoints.SingleOrDefault(item => item.Id == checkpointId));
    }

    public Task<Gate?> GetGateAsync(Guid gateId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Gates.SingleOrDefault(item => item.Id == gateId));
    }
}
