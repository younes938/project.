using System.Security.Claims;
using System.Text.Json;
using ETicket.Api.Security;
using ETicket.Application.OperatingContexts;
using ETicket.Application.Ports;
using ETicket.Domain.Models;

namespace ETicket.Api.OperatingContext;

public sealed record SelectOperatingContextRequest(Guid CheckpointId, Guid GateId);

public sealed record GateOptionResponse(
    Guid Id,
    string Code,
    string Name,
    string Mode);

public sealed record CheckpointOptionResponse(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyCollection<GateOptionResponse> Gates);

public sealed record OperatingContextResponse(
    Guid CheckpointId,
    string CheckpointCode,
    string CheckpointName,
    Guid GateId,
    string GateCode,
    string GateName,
    string Mode,
    DateTimeOffset UpdatedAt);

public static class OperatingContextEndpoints
{
    public static IEndpointRouteBuilder MapOperatingContextEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/operating-context")
            .RequireAuthorization();

        group.MapGet("/options", GetOptionsAsync)
            .WithName("GetOperatingContextOptions");

        group.MapGet("/current", GetCurrentAsync)
            .WithName("GetCurrentOperatingContext");

        group.MapPut("/current", SelectAsync)
            .AddEndpointFilter<AntiforgeryValidationFilter>()
            .WithName("SelectOperatingContext");

        return endpoints;
    }

    private static async Task<IResult> GetOptionsAsync(
        ClaimsPrincipal principal,
        IAuthAdapter authAdapter,
        OperatingContextService service,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(principal, authAdapter, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var options = await service.GetOptionsAsync(user, cancellationToken);
        return Results.Ok(options.Select(option => new CheckpointOptionResponse(
            option.Checkpoint.Id,
            option.Checkpoint.Code,
            option.Checkpoint.Name,
            option.Gates.Select(MapGate).ToArray())));
    }

    private static async Task<IResult> GetCurrentAsync(
        ClaimsPrincipal principal,
        IAuthAdapter authAdapter,
        OperatingContextService service,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(principal, authAdapter, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var current = await service.GetCurrentAsync(user, cancellationToken);
        return current is null
            ? Results.NoContent()
            : Results.Ok(MapContext(current));
    }

    private static async Task<IResult> SelectAsync(
        SelectOperatingContextRequest request,
        HttpContext httpContext,
        IAuthAdapter authAdapter,
        OperatingContextService service,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(httpContext.User, authAdapter, cancellationToken);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        var result = await service.SelectAsync(
            user,
            request.CheckpointId,
            request.GateId,
            httpContext.Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.IsSuccess)
        {
            var statusCode = result.Error?.Code is OperatingContextErrorCodes.GateCheckpointMismatch
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status404NotFound;

            return Results.Problem(
                statusCode: statusCode,
                title: "Operating context could not be selected.",
                detail: result.Error?.Message,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = result.Error?.Code
                });
        }

        return Results.Ok(MapContext(result.Value!));
    }

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

    private static GateOptionResponse MapGate(Gate gate) =>
        new(
            gate.Id,
            gate.Code,
            gate.Name,
            JsonNamingPolicy.CamelCase.ConvertName(gate.Mode.ToString()));

    private static OperatingContextResponse MapContext(SelectedOperatingContext selected) =>
        new(
            selected.Checkpoint.Id,
            selected.Checkpoint.Code,
            selected.Checkpoint.Name,
            selected.Gate.Id,
            selected.Gate.Code,
            selected.Gate.Name,
            JsonNamingPolicy.CamelCase.ConvertName(selected.Context.Mode.ToString()),
            selected.Context.UpdatedAt);
}
