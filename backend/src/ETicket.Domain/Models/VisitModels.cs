using ETicket.Domain.Enums;

namespace ETicket.Domain.Models;

public sealed record ControlResult(
    ControlKind Kind,
    ControlStatus Status,
    DateTimeOffset? DecidedAt,
    Guid? DecidedByUserId,
    string? ReasonCode,
    string? Explanation);

public sealed record Inspection(
    Guid Id,
    Guid VehicleVisitId,
    ControlStatus Status,
    InspectionRejectionReason? RejectionReason,
    string? OtherExplanation,
    Guid? DecidedByUserId,
    DateTimeOffset? DecidedAt);

public sealed record VehicleVisit(
    Guid Id,
    Guid VehicleId,
    Guid CreatedByUserId,
    Guid CheckpointId,
    Guid GateId,
    GateMode GateMode,
    MovementDirection Direction,
    int? PersonsCount,
    string? QueueEntryId,
    VehicleVisitStatus Status,
    IReadOnlyCollection<ControlResult> Controls,
    DateTimeOffset EnteredAt,
    DateTimeOffset? ExitedAt,
    long Version);
