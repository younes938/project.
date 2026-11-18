using Microsoft.EntityFrameworkCore;

namespace ETicket.Api.VehicleEntry.Persistence;

public sealed class VehicleVisitDbContext(DbContextOptions<VehicleVisitDbContext> options) : DbContext(options)
{
    public DbSet<VehicleVisitEntity> VehicleVisits => Set<VehicleVisitEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var visit = modelBuilder.Entity<VehicleVisitEntity>();
        visit.ToTable("VehicleVisits");
        visit.HasKey(x => x.Id);
        visit.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        visit.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        visit.Property(x => x.VehicleJson).HasColumnType("nvarchar(max)").IsRequired();
        visit.Property(x => x.VisitJson).HasColumnType("nvarchar(max)").IsRequired();
        visit.Property(x => x.QueueEntryJson).HasColumnType("nvarchar(max)");
        visit.HasIndex(x => new { x.CreatedByUserId, x.IdempotencyKey }).IsUnique();
        visit.HasIndex(x => new { x.CheckpointId, x.EnteredAt });
    }
}
