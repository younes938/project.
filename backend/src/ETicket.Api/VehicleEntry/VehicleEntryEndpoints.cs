using System.Security.Claims;
using System.Text.Json;
using ETicket.Api.Security;
using ETicket.Application.Ports;
using ETicket.Application.VehicleEntries;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Api.VehicleEntry;

public sealed record CreateVehicleEntryRequest(
    string? PlateNumber,
    string? Brand,
    string? Type,
    bool IsServiceVehicle,
    int? PersonsCount,
    IReadOnlyCollection<string?>? TrailerPlateNumbers,
    string? PhotoReference,
    VehicleEntryDecision Decision,
    string? IdempotencyKey);

public sealed record QueueEntryResponse(
    string Id,
    string PlateNumber,
    string ExternalStatus,
    DateTimeOffset LastSynchronizedAt);

public sealed record QueueLookupResponse(
    string PlateNumber,
    bool Found,
    QueueEntryResponse? Entry);

public sealed record VehicleEntryResponse(
    Guid Id,
    Guid VehicleId,
    Guid CheckpointId,
    Guid GateId,
    string GateMode,
    string Direction,
    string PlateNumber,
    string Brand,
    string Type,
    bool IsServiceVehicle,
    int? PersonsCount,
    IReadOnlyCollection<string> TrailerPlateNumbers,
    string? PhotoReference,
    QueueEntryResponse? QueueEntry,
    string Status,
    DateTimeOffset EnteredAt,
    long Version,
    bool WasCreated);

public static class VehicleEntryEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEntryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/vehicle-entry")
            .RequireAuthorization(PermissionPolicies.For(Permission.ProcessVehicle));

        group.MapGet("/equeue", LookupQueueAsync)
            .WithName("LookupVehicleInEQueue");

        group.MapGet("/{visitId:guid}", GetAsync)
            .WithName("GetVehicleEntry");

        group.MapPost("", CreateAsync)
            .AddEndpointFilter<AntiforgeryValidationFilter>()
            .WithName("CreateVehicleEntry");

        return endpoints;
    }

    private static async Task<IResult> GetAsync(
        Guid visitId,
        ClaimsPrincipal principal,
        IAuthAdapter authAdapter,
        VehicleEntryService service,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(principal, authAdapter, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await service.GetAsync(user, visitId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(MapEntry(result.Value!))
            : MapFailure(result.Error!.Code, result.Error.Message);
    }

    private static async Task<IResult> LookupQueueAsync(
        string? plateNumber,
        ClaimsPrincipal principal,
        IAuthAdapter authAdapter,
        VehicleEntryService service,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(principal, authAdapter, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await service.LookupQueueAsync(user, plateNumber, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(new QueueLookupResponse(
                result.Value!.PlateNumber,
                result.Value.Entry is not null,
                MapQueueEntry(result.Value.Entry)))
            : MapFailure(result.Error!.Code, result.Error.Message);
    }

    private static async Task<IResult> CreateAsync(
        CreateVehicleEntryRequest request,
        HttpContext httpContext,
        IAuthAdapter authAdapter,
        VehicleEntryService service,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(httpContext.User, authAdapter, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await service.CreateAsync(
            user,
            new CreateVehicleEntryCommand(
                request.PlateNumber,
                request.Brand,
                request.Type,
                request.IsServiceVehicle,
                request.PersonsCount,
                request.TrailerPlateNumbers ?? [],
                request.PhotoReference,
                request.Decision,
                request.IdempotencyKey),
            httpContext.Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return MapFailure(result.Error!.Code, result.Error.Message);
        }

        var response = MapEntry(result.Value!);
        return result.Value!.WasCreated
            ? Results.Created($"/api/vehicle-entry/{response.Id:D}", response)
            : Results.Ok(response);
    }

    private static IResult MapFailure(string code, string message)
    {
        var statusCode = code switch
        {
            VehicleEntryErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
            VehicleEntryErrorCodes.OperatingContextRequired => StatusCodes.Status409Conflict,
            VehicleEntryErrorCodes.UnsupportedGateMode => StatusCodes.Status409Conflict,
            VehicleEntryErrorCodes.IdempotencyConflict => StatusCodes.Status409Conflict,
            VehicleEntryErrorCodes.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            statusCode: statusCode,
            title: "Vehicle entry could not be processed.",
            detail: message,
            extensions: new Dictionary<string, object?> { ["code"] = code });
    }

    private static VehicleEntryResponse MapEntry(CreatedVehicleEntry entry) =>
        new(
            entry.Visit.Id,
            entry.Vehicle.Id,
            entry.Visit.CheckpointId,
            entry.Visit.GateId,
            JsonNamingPolicy.CamelCase.ConvertName(entry.Visit.GateMode.ToString()),
            JsonNamingPolicy.CamelCase.ConvertName(entry.Visit.Direction.ToString()),
            entry.Vehicle.PlateNumber,
            entry.Vehicle.Brand,
            entry.Vehicle.Type,
            entry.Vehicle.IsServiceVehicle,
            entry.Visit.PersonsCount,
            entry.Vehicle.Trailers.OrderBy(trailer => trailer.Sequence).Select(trailer => trailer.PlateNumber).ToArray(),
            entry.Vehicle.PhotoReference,
            MapQueueEntry(entry.QueueEntry),
            JsonNamingPolicy.CamelCase.ConvertName(entry.Visit.Status.ToString()),
            entry.Visit.EnteredAt,
            entry.Visit.Version,
            entry.WasCreated);

    private static QueueEntryResponse? MapQueueEntry(QueueEntry? entry) =>
        entry is null
            ? null
            : new QueueEntryResponse(
                entry.Id,
                entry.PlateNumber,
                entry.ExternalStatus,
                entry.LastSynchronizedAt);

    private static async Task<User?> GetUserAsync(
        ClaimsPrincipal principal,
        IAuthAdapter authAdapter,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out var userId)
            ? await authAdapter.GetUserAsync(userId, cancellationToken)
            : null;
    }
}
