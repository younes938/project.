using System.Security.Claims;
using System.Text.Json;
using ETicket.Api.Security;
using ETicket.Application.Ports;
using ETicket.Application.OperatingContexts;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Api.VehicleEntry;

public sealed record VehicleControlSummary(string Kind, string Status, DateTimeOffset? DecidedAt);
public sealed record VehicleVisitSummaryResponse(
    Guid Id, Guid CheckpointId, string PlateNumber, string Status,
    IReadOnlyCollection<VehicleControlSummary> Controls, DateTimeOffset EnteredAt, long Version);

public static class VehicleVisitEndpoints
{
    public static IEndpointRouteBuilder MapVehicleVisitEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/vehicle-visits", async (
            ClaimsPrincipal principal, IAuthAdapter authAdapter,
            OperatingContextService operatingContextService, IVehicleVisitRepository repository,
            CancellationToken cancellationToken) =>
        {
            var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idValue, out var userId)) return Results.Unauthorized();
            var user = await authAdapter.GetUserAsync(userId, cancellationToken);
            if (user is not { IsActive: true }) return Results.Unauthorized();
            if (!user.Roles.SelectMany(role => role.Permissions).Contains(Permission.ViewVehicle) &&
                !user.Roles.SelectMany(role => role.Permissions).Contains(Permission.ProcessVehicle))
                return Results.Forbid();

            var context = await operatingContextService.GetCurrentAsync(user, cancellationToken);
            if (context is null) return Results.Ok(Array.Empty<VehicleVisitSummaryResponse>());
            var visits = await repository.ListByCheckpointAsync(context.Context.CheckpointId, cancellationToken);
            var response = visits.Select(item => new VehicleVisitSummaryResponse(
                item.Visit.Id, item.Visit.CheckpointId, item.Vehicle.PlateNumber,
                JsonNamingPolicy.CamelCase.ConvertName(item.Visit.Status.ToString()),
                item.Visit.Controls.Select(control => new VehicleControlSummary(
                    JsonNamingPolicy.CamelCase.ConvertName(control.Kind.ToString()),
                    JsonNamingPolicy.CamelCase.ConvertName(control.Status.ToString()),
                    control.DecidedAt)).ToArray(),
                item.Visit.EnteredAt, item.Visit.Version)).ToArray();
            return Results.Ok(response);
        }).RequireAuthorization().WithName("ListVehicleVisits");
        return endpoints;
    }
}
