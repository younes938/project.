using ETicket.Domain.Enums;

namespace ETicket.Domain.Models;

public sealed record Trailer(
    Guid Id,
    string PlateNumber,
    int Sequence);

public sealed record Vehicle(
    Guid Id,
    string PlateNumber,
    string Brand,
    string Type,
    bool IsServiceVehicle,
    string? PhotoReference,
    IReadOnlyCollection<Trailer> Trailers);

public sealed record Checkpoint(
    Guid Id,
    string Code,
    string Name,
    bool IsActive);

public sealed record Gate(
    Guid Id,
    Guid CheckpointId,
    string Code,
    string Name,
    GateMode Mode,
    bool IsActive);

public sealed record OperatingContext(
    Guid UserId,
    Guid CheckpointId,
    Guid GateId,
    GateMode Mode,
    DateTimeOffset UpdatedAt);
