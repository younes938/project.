using ETicket.Domain.Enums;
using ETicket.Domain.Models;
using ETicket.Domain.Services;

namespace ETicket.Domain.Tests;

public sealed class OperatingContextSelectionServiceTests
{
    private static readonly Guid CheckpointId = Guid.NewGuid();
    private readonly OperatingContextSelectionService service =
        new(new AccessControlService());

    [Fact]
    public void Validate_AllowsActiveGateAtVisibleActiveCheckpoint()
    {
        var result = service.Validate(
            CreateUser(true, CheckpointId),
            CreateCheckpoint(true),
            CreateGate(CheckpointId, true));

        Assert.Equal(OperatingContextValidationError.None, result);
    }

    [Fact]
    public void Validate_DeniesInactiveUser()
    {
        var result = service.Validate(
            CreateUser(false, CheckpointId),
            CreateCheckpoint(true),
            CreateGate(CheckpointId, true));

        Assert.Equal(OperatingContextValidationError.UserInactive, result);
    }

    [Fact]
    public void Validate_DeniesInactiveCheckpoint()
    {
        var result = service.Validate(
            CreateUser(true, CheckpointId),
            CreateCheckpoint(false),
            CreateGate(CheckpointId, true));

        Assert.Equal(OperatingContextValidationError.CheckpointInactive, result);
    }

    [Fact]
    public void Validate_DeniesCheckpointOutsideVisibilityZone()
    {
        var result = service.Validate(
            CreateUser(true, Guid.NewGuid()),
            CreateCheckpoint(true),
            CreateGate(CheckpointId, true));

        Assert.Equal(OperatingContextValidationError.CheckpointNotVisible, result);
    }

    [Fact]
    public void Validate_DeniesInactiveGate()
    {
        var result = service.Validate(
            CreateUser(true, CheckpointId),
            CreateCheckpoint(true),
            CreateGate(CheckpointId, false));

        Assert.Equal(OperatingContextValidationError.GateInactive, result);
    }

    [Fact]
    public void Validate_DeniesGateFromAnotherCheckpoint()
    {
        var result = service.Validate(
            CreateUser(true, CheckpointId),
            CreateCheckpoint(true),
            CreateGate(Guid.NewGuid(), true));

        Assert.Equal(OperatingContextValidationError.GateDoesNotBelongToCheckpoint, result);
    }

    private static User CreateUser(bool active, Guid visibleCheckpointId) => new(
        Guid.NewGuid(),
        "operator",
        "Operator",
        [],
        [
            new VisibilityZone(
                Guid.NewGuid(),
                "Zone",
                new HashSet<Guid> { visibleCheckpointId })
        ],
        active);

    private static Checkpoint CreateCheckpoint(bool active) =>
        new(CheckpointId, "TEST", "Test checkpoint", active);

    private static Gate CreateGate(Guid checkpointId, bool active) =>
        new(Guid.NewGuid(), checkpointId, "GATE", "Test gate", GateMode.RearEntry, active);
}
