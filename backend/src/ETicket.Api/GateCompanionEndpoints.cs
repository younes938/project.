using System.Collections.Concurrent;
using ETicket.Api.Operations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ETicket.Api;

public sealed record GateDeviceStatus(string DeviceId, string DeviceType, string Status, DateTimeOffset Timestamp, double? Value = null, string? Unit = null, string? Message = null);
public sealed record GateHeartbeat(string GateId, string AgentId, string Mode, DateTimeOffset Timestamp, IReadOnlyList<GateDeviceStatus> Devices);

public sealed class GateCompanionRegistry
{
    private readonly ConcurrentDictionary<string, GateHeartbeat> _gates = new(StringComparer.OrdinalIgnoreCase);
    public void Update(GateHeartbeat heartbeat) => _gates[heartbeat.GateId] = heartbeat;
    public IReadOnlyCollection<GateHeartbeat> GetAll() => _gates.Values.OrderBy(x => x.GateId).ToArray();
}

public static class GateCompanionEndpoints
{
    public static IServiceCollection AddGateCompanion(this IServiceCollection services)
    {
        services.AddSingleton<GateCompanionRegistry>();
        return services;
    }

    public static IEndpointRouteBuilder MapGateCompanionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/gate-companion");

        // Gate Companion runs on the trusted local gate computer. Reads remain behind the user session.
        group.MapPost("/heartbeat", async Task<IResult> (GateHeartbeat heartbeat, GateCompanionRegistry registry, OperationsDbContext db, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(heartbeat.GateId) || string.IsNullOrWhiteSpace(heartbeat.AgentId))
                return Results.BadRequest(new { message = "GateId and AgentId are required." });
            if (heartbeat.Devices is null)
                return Results.BadRequest(new { message = "Devices are required." });

            foreach (var device in heartbeat.Devices.Where(x =>
                string.Equals(x.DeviceType, "weighbridge", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Unit, "kg", StringComparison.OrdinalIgnoreCase) &&
                x.Value.HasValue && double.IsFinite(x.Value.Value) && x.Value.Value > 0 && x.Value.Value <= 1_000_000))
            {
                var weight = Convert.ToDecimal(device.Value!.Value);
                var status = weight > 20_000m ? "OVERWEIGHT" : weight >= 18_000m ? "WARNING" : "NORMAL";
                db.WeightHistory.Add(new WeightHistoryEntity
                {
                    Id = Guid.NewGuid(),
                    GateId = heartbeat.GateId.Trim(),
                    DeviceId = device.DeviceId.Trim(),
                    WeightKg = weight,
                    WeightStatus = status,
                    Timestamp = device.Timestamp == default ? heartbeat.Timestamp : device.Timestamp
                });
            }

            await db.SaveChangesAsync(cancellationToken);
            registry.Update(heartbeat);
            return Results.Accepted();
        });

        group.MapGet("/status", (GateCompanionRegistry registry) => Results.Ok(registry.GetAll()))
            .RequireAuthorization();

        group.MapGet("/weight-history", async (int? limit, OperationsDbContext db, CancellationToken cancellationToken) =>
        {
            var take = Math.Clamp(limit ?? 100, 1, 500);
            var history = await db.WeightHistory.AsNoTracking()
                .OrderByDescending(x => x.Timestamp)
                .Take(take)
                .Select(x => new { x.Id, x.GateId, x.DeviceId, x.WeightKg, x.WeightStatus, x.Timestamp })
                .ToListAsync(cancellationToken);
            return Results.Ok(history);
        }).RequireAuthorization();

        return endpoints;
    }
}
