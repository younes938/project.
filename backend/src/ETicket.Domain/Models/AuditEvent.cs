namespace ETicket.Domain.Models;

public sealed record AuditEvent(
    Guid Id,
    string EventType,
    Guid? UserId,
    string DeviceContext,
    DateTimeOffset Timestamp,
    string EntityType,
    string? EntityId,
    string Action,
    string? PreviousValueJson,
    string? NewValueJson,
    string Result);
