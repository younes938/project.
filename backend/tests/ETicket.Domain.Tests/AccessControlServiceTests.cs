using ETicket.Domain.Enums;
using ETicket.Domain.Models;
using ETicket.Domain.Services;

namespace ETicket.Domain.Tests;

public sealed class AccessControlServiceTests
{
    private static readonly Guid AllowedCheckpointId = Guid.NewGuid();
    private readonly AccessControlService service = new();

    [Fact]
    public void HasPermission_ReturnsTrue_WhenAnyRoleGrantsPermission()
    {
        var user = CreateUser(true);

        Assert.True(service.HasPermission(user, Permission.ViewVehicle));
    }

    [Fact]
    public void HasPermission_ReturnsFalse_WhenPermissionIsNotGranted()
    {
        var user = CreateUser(true);

        Assert.False(service.HasPermission(user, Permission.ExportVehicles));
    }

    [Fact]
    public void CanAccessCheckpoint_ReturnsTrue_WhenCheckpointBelongsToVisibilityZone()
    {
        var user = CreateUser(true);

        Assert.True(service.CanAccessCheckpoint(user, AllowedCheckpointId));
    }

    [Fact]
    public void InactiveUser_HasNoPermissionsOrVisibleCheckpoints()
    {
        var user = CreateUser(false);

        Assert.False(service.HasPermission(user, Permission.ViewVehicle));
        Assert.False(service.CanAccessCheckpoint(user, AllowedCheckpointId));
        Assert.Empty(service.GetVisibleCheckpointIds(user));
    }

    [Fact]
    public void GetVisibleCheckpointIds_ReturnsDistinctUnionAcrossZones()
    {
        var secondCheckpointId = Guid.NewGuid();
        var user = CreateUser(true) with
        {
            VisibilityZones =
            [
                CreateZone(Guid.NewGuid(), AllowedCheckpointId),
                CreateZone(Guid.NewGuid(), AllowedCheckpointId, secondCheckpointId)
            ]
        };

        var result = service.GetVisibleCheckpointIds(user);

        Assert.Equal(2, result.Count);
        Assert.Contains(AllowedCheckpointId, result);
        Assert.Contains(secondCheckpointId, result);
    }

    private static User CreateUser(bool isActive) => new(
        Guid.NewGuid(),
        "operator-1",
        "Operator",
        [
            new Role(
                Guid.NewGuid(),
                "operator",
                "Operator",
                new HashSet<Permission> { Permission.ViewVehicle })
        ],
        [CreateZone(Guid.NewGuid(), AllowedCheckpointId)],
        isActive);

    private static VisibilityZone CreateZone(Guid id, params Guid[] checkpointIds) =>
        new(id, "Zone", checkpointIds.ToHashSet());
}
