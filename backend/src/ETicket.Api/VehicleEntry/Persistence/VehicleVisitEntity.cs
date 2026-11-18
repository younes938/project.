namespace ETicket.Api.VehicleEntry.Persistence;

public sealed class VehicleVisitEntity
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid CheckpointId { get; set; }
    public DateTimeOffset EnteredAt { get; set; }
    public long Version { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string RequestFingerprint { get; set; } = string.Empty;
    public string VehicleJson { get; set; } = string.Empty;
    public string VisitJson { get; set; } = string.Empty;
    public string? QueueEntryJson { get; set; }
}
