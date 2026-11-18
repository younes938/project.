using System.Security.Claims;
using ETicket.Api.Operations.Persistence;
using ETicket.Api.Passengers.Persistence;
using ETicket.Api.VehicleEntry.Persistence;
using ETicket.Api.Security;
using ETicket.Application.Ports;
using ETicket.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ETicket.Api.Operations;

public sealed record ShipmentDraft(string ShipmentNumber, string GoodsType, decimal WeightKg, string OriginCountry, string DestinationCountry, string CustomsStatus, string Notes);
public sealed record InspectionDraft(string TargetType, string TargetReference, string Notes, string Status, bool SecurityAlert, string AlertDetails);

public static class OperationsEndpoints
{
    private static readonly string[] CustomsStatuses = ["Approved", "Denied", "Under Inspection"];
    private static readonly string[] InspectionStatuses = ["Clear", "Suspicious", "Requires Review"];
    private static readonly string[] TargetTypes = ["Passenger", "Vehicle", "Shipment"];

    public static IEndpointRouteBuilder MapOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var shipments = endpoints.MapGroup("/api/shipments").RequireAuthorization();
        shipments.MapGet("", async (OperationsDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Shipments.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct)));
        shipments.MapPost("", async (ShipmentDraft draft, OperationsDbContext db, IAuditSink audit, HttpContext http, CancellationToken ct) =>
        {
            var errors = ValidateShipment(draft);
            if (errors is not null) return Results.ValidationProblem(errors);
            var number = draft.ShipmentNumber.Trim();
            if (await db.Shipments.AnyAsync(x => x.ShipmentNumber == number, ct))
                return Results.Conflict(new { message = "رقم الشحنة مسجل مسبقًا." });
            var now = DateTimeOffset.UtcNow;
            var entity = new ShipmentEntity { Id = Guid.NewGuid(), ShipmentNumber = number, GoodsType = draft.GoodsType.Trim(), WeightKg = draft.WeightKg, OriginCountry = draft.OriginCountry.Trim(), DestinationCountry = draft.DestinationCountry.Trim(), CustomsStatus = draft.CustomsStatus, Notes = draft.Notes?.Trim() ?? "", CreatedAt = now, UpdatedAt = now };
            db.Shipments.Add(entity); await db.SaveChangesAsync(ct);
            await WriteAudit(audit, http, "Shipment", entity.Id.ToString(), "Create", ct);
            return Results.Created($"/api/shipments/{entity.Id:D}", entity);
        }).AddEndpointFilter<AntiforgeryValidationFilter>();
        shipments.MapPut("/{id:guid}", async (Guid id, ShipmentDraft draft, OperationsDbContext db, IAuditSink audit, HttpContext http, CancellationToken ct) =>
        {
            var errors = ValidateShipment(draft);
            if (errors is not null) return Results.ValidationProblem(errors);
            var entity = await db.Shipments.FindAsync(new object?[] { id }, ct);
            if (entity is null) return Results.NotFound();
            var number = draft.ShipmentNumber.Trim();
            if (await db.Shipments.AnyAsync(x => x.Id != id && x.ShipmentNumber == number, ct)) return Results.Conflict(new { message = "رقم الشحنة مسجل مسبقًا." });
            entity.ShipmentNumber = number; entity.GoodsType = draft.GoodsType.Trim(); entity.WeightKg = draft.WeightKg; entity.OriginCountry = draft.OriginCountry.Trim(); entity.DestinationCountry = draft.DestinationCountry.Trim(); entity.CustomsStatus = draft.CustomsStatus; entity.Notes = draft.Notes?.Trim() ?? ""; entity.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct); await WriteAudit(audit, http, "Shipment", id.ToString(), "Update", ct); return Results.Ok(entity);
        }).AddEndpointFilter<AntiforgeryValidationFilter>();
        shipments.MapDelete("/{id:guid}", async (Guid id, OperationsDbContext db, IAuditSink audit, HttpContext http, CancellationToken ct) =>
        { var entity = await db.Shipments.FindAsync(new object?[] { id }, ct); if (entity is null) return Results.NotFound(); db.Remove(entity); await db.SaveChangesAsync(ct); await WriteAudit(audit, http, "Shipment", id.ToString(), "Delete", ct); return Results.NoContent(); }).AddEndpointFilter<AntiforgeryValidationFilter>();

        var inspections = endpoints.MapGroup("/api/inspections").RequireAuthorization();
        inspections.MapGet("", async (OperationsDbContext db, CancellationToken ct) => Results.Ok(await db.Inspections.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(ct)));
        inspections.MapPost("", async (InspectionDraft draft, OperationsDbContext db, IAuditSink audit, HttpContext http, CancellationToken ct) =>
        {
            var errors = ValidateInspection(draft); if (errors is not null) return Results.ValidationProblem(errors);
            var entity = new InspectionEntity { Id = Guid.NewGuid(), TargetType = draft.TargetType, TargetReference = draft.TargetReference.Trim(), Notes = draft.Notes.Trim(), Status = draft.Status, SecurityAlert = draft.SecurityAlert, AlertDetails = draft.AlertDetails?.Trim() ?? "", CreatedAt = DateTimeOffset.UtcNow, CreatedBy = http.User.Identity?.Name ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown" };
            db.Inspections.Add(entity); await db.SaveChangesAsync(ct); await WriteAudit(audit, http, "Inspection", entity.Id.ToString(), "Create", ct); return Results.Created($"/api/inspections/{entity.Id:D}", entity);
        }).AddEndpointFilter<AntiforgeryValidationFilter>();
        inspections.MapPut("/{id:guid}", async (Guid id, InspectionDraft draft, OperationsDbContext db, IAuditSink audit, HttpContext http, CancellationToken ct) =>
        {
            var errors = ValidateInspection(draft); if (errors is not null) return Results.ValidationProblem(errors);
            var entity = await db.Inspections.FindAsync(new object?[] { id }, ct); if (entity is null) return Results.NotFound();
            entity.TargetType = draft.TargetType; entity.TargetReference = draft.TargetReference.Trim(); entity.Notes = draft.Notes.Trim(); entity.Status = draft.Status; entity.SecurityAlert = draft.SecurityAlert; entity.AlertDetails = draft.AlertDetails?.Trim() ?? "";
            await db.SaveChangesAsync(ct); await WriteAudit(audit, http, "Inspection", id.ToString(), "Update", ct); return Results.Ok(entity);
        }).AddEndpointFilter<AntiforgeryValidationFilter>();
        inspections.MapDelete("/{id:guid}", async (Guid id, OperationsDbContext db, IAuditSink audit, HttpContext http, CancellationToken ct) =>
        { var entity = await db.Inspections.FindAsync(new object?[] { id }, ct); if (entity is null) return Results.NotFound(); db.Remove(entity); await db.SaveChangesAsync(ct); await WriteAudit(audit, http, "Inspection", id.ToString(), "Delete", ct); return Results.NoContent(); }).AddEndpointFilter<AntiforgeryValidationFilter>();

        endpoints.MapGet("/api/reports/summary", async (OperationsDbContext ops, PassengerDbContext passengers, VehicleVisitDbContext vehicles, CancellationToken ct) => Results.Ok(new
        {
            passengers = await passengers.Passengers.CountAsync(ct),
            vehicles = await vehicles.VehicleVisits.CountAsync(ct),
            shipments = await ops.Shipments.CountAsync(ct),
            inspections = await ops.Inspections.CountAsync(ct),
            securityAlerts = await ops.Inspections.CountAsync(x => x.SecurityAlert, ct),
            suspiciousInspections = await ops.Inspections.CountAsync(x => x.Status != "Clear", ct),
            shipmentsPending = await ops.Shipments.CountAsync(x => x.CustomsStatus == "Under Inspection", ct),
            generatedAt = DateTimeOffset.UtcNow
        })).RequireAuthorization().WithName("ReportsSummary");

        endpoints.MapGet("/api/audit", async (OperationsDbContext db, int? limit, CancellationToken ct) =>
            Results.Ok(await db.AuditRecords.AsNoTracking().OrderByDescending(x => x.Timestamp).Take(Math.Clamp(limit ?? 100, 1, 500)).ToListAsync(ct)))
            .RequireAuthorization().WithName("ListAuditRecords");
        return endpoints;
    }

    private static Dictionary<string, string[]>? ValidateShipment(ShipmentDraft d)
    {
        var e = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(d.ShipmentNumber) || d.ShipmentNumber.Trim().Length > 60) e[nameof(d.ShipmentNumber)] = ["Shipment number is required and must not exceed 60 characters."];
        if (string.IsNullOrWhiteSpace(d.GoodsType) || d.GoodsType.Trim().Length > 120) e[nameof(d.GoodsType)] = ["Goods type is required and must not exceed 120 characters."];
        if (d.WeightKg <= 0 || d.WeightKg > 100000000) e[nameof(d.WeightKg)] = ["Weight must be greater than zero."];
        if (string.IsNullOrWhiteSpace(d.OriginCountry) || d.OriginCountry.Trim().Length > 80) e[nameof(d.OriginCountry)] = ["Origin country is required."];
        if (string.IsNullOrWhiteSpace(d.DestinationCountry) || d.DestinationCountry.Trim().Length > 80) e[nameof(d.DestinationCountry)] = ["Destination country is required."];
        if (!CustomsStatuses.Contains(d.CustomsStatus)) e[nameof(d.CustomsStatus)] = ["Customs status is invalid."];
        if ((d.Notes?.Length ?? 0) > 1000) e[nameof(d.Notes)] = ["Notes exceed the allowed length."];
        return e.Count == 0 ? null : e;
    }
    private static Dictionary<string, string[]>? ValidateInspection(InspectionDraft d)
    {
        var e = new Dictionary<string, string[]>();
        if (!TargetTypes.Contains(d.TargetType)) e[nameof(d.TargetType)] = ["Target type is invalid."];
        if (string.IsNullOrWhiteSpace(d.TargetReference) || d.TargetReference.Trim().Length > 100) e[nameof(d.TargetReference)] = ["Passenger, plate, or shipment reference is required."];
        if (string.IsNullOrWhiteSpace(d.Notes) || d.Notes.Trim().Length > 2000) e[nameof(d.Notes)] = ["Inspection notes are required and must not exceed 2000 characters."];
        if (!InspectionStatuses.Contains(d.Status)) e[nameof(d.Status)] = ["Inspection status is invalid."];
        if ((d.AlertDetails?.Length ?? 0) > 1000) e[nameof(d.AlertDetails)] = ["Alert details exceed the allowed length."];
        if (d.SecurityAlert && string.IsNullOrWhiteSpace(d.AlertDetails)) e[nameof(d.AlertDetails)] = ["Please provide details for the security alert."];
        return e.Count == 0 ? null : e;
    }
    private static Task WriteAudit(IAuditSink sink, HttpContext http, string entity, string id, string action, CancellationToken ct) =>
        sink.AppendAsync(new AuditEvent(Guid.NewGuid(), "DataChange", Guid.TryParse(http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null, (http.Request.Headers.UserAgent.ToString() is { Length: > 200 } agent ? agent[..200] : http.Request.Headers.UserAgent.ToString()), DateTimeOffset.UtcNow, entity, id, action, null, null, "Success"), ct);
}
