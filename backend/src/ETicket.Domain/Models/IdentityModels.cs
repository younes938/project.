using ETicket.Domain.Enums;

namespace ETicket.Domain.Models;

public sealed record Role(
    Guid Id,
    string Code,
    string Name,
    IReadOnlySet<Permission> Permissions);

public sealed record VisibilityZone(
    Guid Id,
    string Name,
    IReadOnlySet<Guid> CheckpointIds);

public sealed record User(
    Guid Id,
    string ExternalId,
    string DisplayName,
    IReadOnlyCollection<Role> Roles,
    IReadOnlyCollection<VisibilityZone> VisibilityZones,
    bool IsActive);
