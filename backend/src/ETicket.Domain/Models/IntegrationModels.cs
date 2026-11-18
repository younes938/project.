namespace ETicket.Domain.Models;

public sealed record QueueEntry(
    string Id,
    Guid CheckpointId,
    string PlateNumber,
    string ExternalStatus,
    DateTimeOffset LastSynchronizedAt);

public sealed record CameraRecognition(
    string Id,
    string CameraId,
    string Location,
    DateTimeOffset CapturedAt,
    string PlateNumber,
    string? PhotoReference,
    DateTimeOffset ExpiresAt);
