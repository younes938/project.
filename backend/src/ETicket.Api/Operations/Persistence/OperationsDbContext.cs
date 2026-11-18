using Microsoft.EntityFrameworkCore;

namespace ETicket.Api.Operations.Persistence;

public sealed class ShipmentEntity
{
    public Guid Id { get; set; }
    public string ShipmentNumber { get; set; } = "";
    public string GoodsType { get; set; } = "";
    public decimal WeightKg { get; set; }
    public string OriginCountry { get; set; } = "";
    public string DestinationCountry { get; set; } = "";
    public string CustomsStatus { get; set; } = "Under Inspection";
    public string Notes { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class InspectionEntity
{
    public Guid Id { get; set; }
    public string TargetType { get; set; } = "Vehicle";
    public string TargetReference { get; set; } = "";
    public string Notes { get; set; } = "";
    public string Status { get; set; } = "Requires Review";
    public bool SecurityAlert { get; set; }
    public string AlertDetails { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
}

public sealed class AuditRecordEntity
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = "";
    public Guid? UserId { get; set; }
    public string DeviceContext { get; set; } = "";
    public DateTimeOffset Timestamp { get; set; }
    public string EntityType { get; set; } = "";
    public string? EntityId { get; set; }
    public string Action { get; set; } = "";
    public string? PreviousValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public string Result { get; set; } = "Success";
}


public sealed class WeightHistoryEntity
{
    public Guid Id { get; set; }
    public string GateId { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public decimal WeightKg { get; set; }
    public string WeightStatus { get; set; } = "NORMAL";
    public DateTimeOffset Timestamp { get; set; }
}

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options) : DbContext(options)
{
    public DbSet<ShipmentEntity> Shipments => Set<ShipmentEntity>();
    public DbSet<InspectionEntity> Inspections => Set<InspectionEntity>();
    public DbSet<AuditRecordEntity> AuditRecords => Set<AuditRecordEntity>();
    public DbSet<WeightHistoryEntity> WeightHistory => Set<WeightHistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var shipments = modelBuilder.Entity<ShipmentEntity>();
        shipments.ToTable("Shipments"); shipments.HasKey(x => x.Id);
        shipments.Property(x => x.ShipmentNumber).HasMaxLength(60).IsRequired();
        shipments.HasIndex(x => x.ShipmentNumber).IsUnique();
        shipments.Property(x => x.GoodsType).HasMaxLength(120).IsRequired();
        shipments.Property(x => x.WeightKg).HasPrecision(18, 3);
        shipments.Property(x => x.OriginCountry).HasMaxLength(80).IsRequired();
        shipments.Property(x => x.DestinationCountry).HasMaxLength(80).IsRequired();
        shipments.Property(x => x.CustomsStatus).HasMaxLength(24).IsRequired();
        shipments.Property(x => x.Notes).HasMaxLength(1000);

        var inspections = modelBuilder.Entity<InspectionEntity>();
        inspections.ToTable("Inspections"); inspections.HasKey(x => x.Id);
        inspections.Property(x => x.TargetType).HasMaxLength(24).IsRequired();
        inspections.Property(x => x.TargetReference).HasMaxLength(100).IsRequired();
        inspections.Property(x => x.Notes).HasMaxLength(2000).IsRequired();
        inspections.Property(x => x.Status).HasMaxLength(24).IsRequired();
        inspections.Property(x => x.AlertDetails).HasMaxLength(1000).IsRequired();
        inspections.Property(x => x.CreatedBy).HasMaxLength(120).IsRequired();
        inspections.HasIndex(x => x.CreatedAt);

        var audit = modelBuilder.Entity<AuditRecordEntity>();
        audit.ToTable("AuditRecords"); audit.HasKey(x => x.Id);
        audit.Property(x => x.EventType).HasMaxLength(80).IsRequired();
        audit.Property(x => x.DeviceContext).HasMaxLength(200).IsRequired();
        audit.Property(x => x.EntityType).HasMaxLength(80).IsRequired();
        audit.Property(x => x.EntityId).HasMaxLength(100);
        audit.Property(x => x.Action).HasMaxLength(80).IsRequired();
        audit.Property(x => x.Result).HasMaxLength(40).IsRequired();
        audit.HasIndex(x => x.Timestamp);

        var weightHistory = modelBuilder.Entity<WeightHistoryEntity>();
        weightHistory.ToTable("WeightHistory");
        weightHistory.HasKey(x => x.Id);
        weightHistory.Property(x => x.GateId).HasMaxLength(80).IsRequired();
        weightHistory.Property(x => x.DeviceId).HasMaxLength(80).IsRequired();
        weightHistory.Property(x => x.WeightKg).HasPrecision(18, 3);
        weightHistory.Property(x => x.WeightStatus).HasMaxLength(20).IsRequired();
        weightHistory.HasIndex(x => x.Timestamp);
        weightHistory.HasIndex(x => new { x.GateId, x.Timestamp });
    }
}
