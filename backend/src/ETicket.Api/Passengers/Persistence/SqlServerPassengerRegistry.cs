using Microsoft.EntityFrameworkCore;

namespace ETicket.Api.Passengers.Persistence;

public sealed class SqlServerPassengerRegistry(PassengerDbContext dbContext) : IPassengerRegistry
{
    public async Task<IReadOnlyCollection<PassengerResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var entities = await dbContext.Passengers.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);
        return entities.Select(ToResponse).ToArray();
    }

    public async Task<PassengerResponse> AddAsync(PassengerDraft draft, CancellationToken cancellationToken)
    {
        var entity = new PassengerEntity
        {
            Id = Guid.NewGuid(),
            FullName = draft.FullName.Trim(),
            PassportNumber = draft.PassportNumber.Trim(),
            Nationality = draft.Nationality.Trim(),
            OriginCountry = draft.OriginCountry.Trim(),
            DestinationCountry = draft.DestinationCountry.Trim(),
            Status = draft.Status,
            Direction = draft.Direction,
            Notes = draft.Notes?.Trim() ?? string.Empty,
            CreatedAt = DateTimeOffset.UtcNow
        };
        dbContext.Passengers.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<PassengerResponse?> UpdateAsync(Guid id, PassengerDraft draft, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Passengers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        entity.FullName = draft.FullName.Trim();
        entity.PassportNumber = draft.PassportNumber.Trim();
        entity.Nationality = draft.Nationality.Trim();
        entity.OriginCountry = draft.OriginCountry.Trim();
        entity.DestinationCountry = draft.DestinationCountry.Trim();
        entity.Status = draft.Status;
        entity.Direction = draft.Direction;
        entity.Notes = draft.Notes?.Trim() ?? string.Empty;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Passengers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;
        dbContext.Passengers.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static PassengerResponse ToResponse(PassengerEntity entity) => new(
        entity.Id, entity.FullName, entity.PassportNumber, entity.Nationality,
        entity.OriginCountry, entity.DestinationCountry, entity.Status, entity.Direction,
        entity.Notes, entity.CreatedAt);
}
