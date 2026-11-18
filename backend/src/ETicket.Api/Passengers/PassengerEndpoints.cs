using System.Collections.Concurrent;
using ETicket.Api.Security;

namespace ETicket.Api.Passengers;

public sealed record PassengerDraft(
    string FullName,
    string PassportNumber,
    string Nationality,
    string OriginCountry,
    string DestinationCountry,
    string Status,
    string Direction,
    string Notes);

public sealed record PassengerResponse(
    Guid Id,
    string FullName,
    string PassportNumber,
    string Nationality,
    string OriginCountry,
    string DestinationCountry,
    string Status,
    string Direction,
    string Notes,
    DateTimeOffset CreatedAt);

public interface IPassengerRegistry
{
    Task<IReadOnlyCollection<PassengerResponse>> ListAsync(CancellationToken cancellationToken);
    Task<PassengerResponse> AddAsync(PassengerDraft draft, CancellationToken cancellationToken);
    Task<PassengerResponse?> UpdateAsync(Guid id, PassengerDraft draft, CancellationToken cancellationToken);
    Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class InMemoryPassengerRegistry : IPassengerRegistry
{
    private readonly ConcurrentDictionary<Guid, PassengerResponse> _records = new();

    public Task<IReadOnlyCollection<PassengerResponse>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<PassengerResponse>>(
            _records.Values.OrderByDescending(passenger => passenger.CreatedAt).ToArray());

    public Task<PassengerResponse> AddAsync(PassengerDraft draft, CancellationToken cancellationToken)
    {
        var record = new PassengerResponse(
            Guid.NewGuid(), draft.FullName.Trim(), draft.PassportNumber.Trim(), draft.Nationality.Trim(),
            draft.OriginCountry.Trim(), draft.DestinationCountry.Trim(), draft.Status, draft.Direction,
            draft.Notes?.Trim() ?? string.Empty, DateTimeOffset.UtcNow);
        _records[record.Id] = record;
        return Task.FromResult(record);
    }

    public Task<PassengerResponse?> UpdateAsync(Guid id, PassengerDraft draft, CancellationToken cancellationToken)
    {
        if (!_records.TryGetValue(id, out var existing))
            return Task.FromResult<PassengerResponse?>(null);
        var updated = existing with
        {
            FullName = draft.FullName.Trim(), PassportNumber = draft.PassportNumber.Trim(),
            Nationality = draft.Nationality.Trim(), OriginCountry = draft.OriginCountry.Trim(),
            DestinationCountry = draft.DestinationCountry.Trim(), Status = draft.Status,
            Direction = draft.Direction, Notes = draft.Notes?.Trim() ?? string.Empty
        };
        _records[id] = updated;
        return Task.FromResult<PassengerResponse?>(updated);
    }

    public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_records.TryRemove(id, out _));
}

public static class PassengerEndpoints
{
    private static readonly HashSet<string> AllowedStatuses = ["Allowed", "Denied", "Pending Verification"];
    private static readonly HashSet<string> AllowedDirections = ["Entry", "Exit"];

    public static IEndpointRouteBuilder MapPassengerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/passengers").RequireAuthorization();
        group.MapGet("", async (IPassengerRegistry registry, CancellationToken cancellationToken) =>
                Results.Ok(await registry.ListAsync(cancellationToken)))
            .WithName("ListPassengers");
        group.MapPost("", async (PassengerDraft draft, IPassengerRegistry registry, CancellationToken cancellationToken) =>
        {
            var validation = Validate(draft);
            if (validation is not null) return Results.ValidationProblem(validation);
            var record = await registry.AddAsync(draft, cancellationToken);
            return Results.Created($"/api/passengers/{record.Id:D}", record);
        }).AddEndpointFilter<AntiforgeryValidationFilter>().WithName("CreatePassenger");
        group.MapPut("/{id:guid}", async (Guid id, PassengerDraft draft, IPassengerRegistry registry, CancellationToken cancellationToken) =>
        {
            var validation = Validate(draft);
            if (validation is not null) return Results.ValidationProblem(validation);
            var record = await registry.UpdateAsync(id, draft, cancellationToken);
            return record is null ? Results.NotFound() : Results.Ok(record);
        }).AddEndpointFilter<AntiforgeryValidationFilter>().WithName("UpdatePassenger");
        group.MapDelete("/{id:guid}", async (Guid id, IPassengerRegistry registry, CancellationToken cancellationToken) =>
            await registry.RemoveAsync(id, cancellationToken) ? Results.NoContent() : Results.NotFound())
            .AddEndpointFilter<AntiforgeryValidationFilter>().WithName("DeletePassenger");
        return endpoints;
    }

    private static Dictionary<string, string[]>? Validate(PassengerDraft draft)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(draft.FullName) || draft.FullName.Trim().Length > 120)
            errors[nameof(draft.FullName)] = ["Full name is required and must not exceed 120 characters."];
        if (string.IsNullOrWhiteSpace(draft.PassportNumber) || draft.PassportNumber.Trim().Length > 40)
            errors[nameof(draft.PassportNumber)] = ["Passport number is required and must not exceed 40 characters."];
        if (string.IsNullOrWhiteSpace(draft.Nationality) || draft.Nationality.Trim().Length > 80)
            errors[nameof(draft.Nationality)] = ["Nationality is required and must not exceed 80 characters."];
        if (string.IsNullOrWhiteSpace(draft.OriginCountry) || draft.OriginCountry.Trim().Length > 80)
            errors[nameof(draft.OriginCountry)] = ["Origin country is required and must not exceed 80 characters."];
        if (string.IsNullOrWhiteSpace(draft.DestinationCountry) || draft.DestinationCountry.Trim().Length > 80)
            errors[nameof(draft.DestinationCountry)] = ["Destination country is required and must not exceed 80 characters."];
        if (!AllowedStatuses.Contains(draft.Status)) errors[nameof(draft.Status)] = ["Crossing status is invalid."];
        if (!AllowedDirections.Contains(draft.Direction)) errors[nameof(draft.Direction)] = ["Travel direction is invalid."];
        if ((draft.Notes?.Length ?? 0) > 500) errors[nameof(draft.Notes)] = ["Notes must not exceed 500 characters."];
        return errors.Count == 0 ? null : errors;
    }
}
