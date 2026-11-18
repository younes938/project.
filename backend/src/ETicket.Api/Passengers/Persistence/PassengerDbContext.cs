using Microsoft.EntityFrameworkCore;

namespace ETicket.Api.Passengers.Persistence;

public sealed class PassengerDbContext(DbContextOptions<PassengerDbContext> options) : DbContext(options)
{
    public DbSet<PassengerEntity> Passengers => Set<PassengerEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var passenger = modelBuilder.Entity<PassengerEntity>();
        passenger.ToTable("Passengers");
        passenger.HasKey(x => x.Id);
        passenger.Property(x => x.FullName).HasMaxLength(120).IsRequired();
        passenger.Property(x => x.PassportNumber).HasMaxLength(40).IsRequired();
        passenger.Property(x => x.Nationality).HasMaxLength(80).IsRequired();
        passenger.Property(x => x.OriginCountry).HasMaxLength(80).IsRequired();
        passenger.Property(x => x.DestinationCountry).HasMaxLength(80).IsRequired();
        passenger.Property(x => x.Status).HasMaxLength(24).IsRequired();
        passenger.Property(x => x.Direction).HasMaxLength(12).IsRequired();
        passenger.Property(x => x.Notes).HasMaxLength(500).IsRequired();
        passenger.Property(x => x.CreatedAt).IsRequired();
        passenger.HasIndex(x => x.PassportNumber);
        passenger.HasIndex(x => x.CreatedAt);
    }
}
