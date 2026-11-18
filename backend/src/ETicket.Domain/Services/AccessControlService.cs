using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Domain.Services;

public sealed class AccessControlService
{
    public bool HasPermission(User user, Permission permission) =>
        user.IsActive && user.Roles.Any(role => role.Permissions.Contains(permission));

    public bool CanAccessCheckpoint(User user, Guid checkpointId) =>
        user.IsActive &&
        user.VisibilityZones.Any(zone => zone.CheckpointIds.Contains(checkpointId));

    public IReadOnlySet<Guid> GetVisibleCheckpointIds(User user)
    {
        if (!user.IsActive)
        {
            return new HashSet<Guid>();
        }

        return user.VisibilityZones
            .SelectMany(zone => zone.CheckpointIds)
            .ToHashSet();
    }
}
