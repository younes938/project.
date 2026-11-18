using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ETicket.Application.Common;
using ETicket.Application.OperatingContexts;
using ETicket.Application.Ports;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;
using ETicket.Domain.Services;

namespace ETicket.Application.VehicleEntries;

public sealed record CreateVehicleEntryCommand(
    string? PlateNumber,
    string? Brand,
    string? Type,
    bool IsServiceVehicle,
    int? PersonsCount,
    IReadOnlyCollection<string?> TrailerPlateNumbers,
    string? PhotoReference,
    VehicleEntryDecision Decision,
    string? IdempotencyKey);

public sealed record QueueLookup(
    string PlateNumber,
    QueueEntry? Entry);

public sealed record CreatedVehicleEntry(
    Vehicle Vehicle,
    VehicleVisit Visit,
    QueueEntry? QueueEntry,
    bool WasCreated);

public static class VehicleEntryErrorCodes
{
    public const string Forbidden = "vehicle_entry.forbidden";
    public const string OperatingContextRequired = "vehicle_entry.operating_context_required";
    public const string UnsupportedGateMode = "vehicle_entry.unsupported_gate_mode";
    public const string InvalidPlate = "vehicle_entry.invalid_plate";
    public const string InvalidInput = "vehicle_entry.invalid_input";
    public const string InvalidIdempotencyKey = "vehicle_entry.invalid_idempotency_key";
    public const string IdempotencyConflict = "vehicle_entry.idempotency_conflict";
    public const string NotFound = "vehicle_entry.not_found";
}

public sealed class VehicleEntryService(
    IVehicleVisitRepository visitRepository,
    OperatingContextService operatingContextService,
    IEQueueAdapter queueAdapter,
    VehicleEntryValidationService validationService,
    AccessControlService accessControl,
    IClock clock,
    IAuditSink auditSink)
{
    public async Task<OperationResult<CreatedVehicleEntry>> GetAsync(
        User user,
        Guid visitId,
        CancellationToken cancellationToken)
    {
        if (!accessControl.HasPermission(user, Permission.ProcessVehicle))
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                VehicleEntryErrorCodes.Forbidden,
                "Vehicle processing permission is required.");
        }

        var stored = await visitRepository.GetByIdAsync(visitId, cancellationToken);
        if (stored is null || !accessControl.CanAccessCheckpoint(user, stored.Visit.CheckpointId))
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                VehicleEntryErrorCodes.NotFound,
                "Vehicle entry was not found.");
        }

        return OperationResult<CreatedVehicleEntry>.Success(
            new CreatedVehicleEntry(
                stored.Vehicle,
                stored.Visit,
                stored.QueueEntry,
                false));
    }

    public async Task<OperationResult<QueueLookup>> LookupQueueAsync(
        User user,
        string? plateNumber,
        CancellationToken cancellationToken)
    {
        var contextResult = await GetEntryContextAsync(user, cancellationToken);
        if (!contextResult.IsSuccess)
        {
            return OperationResult<QueueLookup>.Failure(
                contextResult.Error!.Code,
                contextResult.Error.Message);
        }

        var normalizedPlate = validationService.NormalizePlate(plateNumber);
        if (!validationService.IsValidPlate(normalizedPlate))
        {
            return OperationResult<QueueLookup>.Failure(
                VehicleEntryErrorCodes.InvalidPlate,
                "Plate number must contain 2 to 16 letters or digits.");
        }

        var entry = await queueAdapter.FindByPlateAsync(
            contextResult.Value!.Context.CheckpointId,
            normalizedPlate,
            cancellationToken);

        return OperationResult<QueueLookup>.Success(new QueueLookup(normalizedPlate, entry));
    }

    public async Task<OperationResult<CreatedVehicleEntry>> CreateAsync(
        User user,
        CreateVehicleEntryCommand command,
        string deviceContext,
        CancellationToken cancellationToken)
    {
        var contextResult = await GetEntryContextAsync(user, cancellationToken);
        if (!contextResult.IsSuccess)
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                contextResult.Error!.Code,
                contextResult.Error.Message);
        }

        var idempotencyKey = command.IdempotencyKey?.Trim() ?? string.Empty;
        if (command.Decision is not VehicleEntryDecision.Allow and not VehicleEntryDecision.Deny)
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                VehicleEntryErrorCodes.InvalidInput,
                "An explicit allow or deny decision is required.");
        }

        if (idempotencyKey.Length is < 8 or > 100)
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                VehicleEntryErrorCodes.InvalidIdempotencyKey,
                "Idempotency key must contain 8 to 100 characters.");
        }

        var validation = validationService.Validate(new VehicleEntryDraft(
            command.PlateNumber,
            command.Brand,
            command.Type,
            command.IsServiceVehicle,
            command.PersonsCount,
            command.TrailerPlateNumbers,
            command.PhotoReference));

        if (!validation.IsValid)
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                VehicleEntryErrorCodes.InvalidInput,
                string.Join(" ", validation.Issues.Select(issue => issue.Message).Distinct()));
        }

        var normalized = validation.Value!;
        var context = contextResult.Value!.Context;
        var queueEntry = await queueAdapter.FindByPlateAsync(
            context.CheckpointId,
            normalized.PlateNumber,
            cancellationToken);
        var enteredAt = clock.UtcNow;
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle(
            vehicleId,
            normalized.PlateNumber,
            normalized.Brand,
            normalized.Type,
            normalized.IsServiceVehicle,
            normalized.PhotoReference,
            normalized.TrailerPlateNumbers
                .Select((plate, index) => new Trailer(Guid.NewGuid(), plate, index + 1))
                .ToArray());
        var visit = new VehicleVisit(
            Guid.NewGuid(),
            vehicleId,
            user.Id,
            context.CheckpointId,
            context.GateId,
            context.Mode,
            validationService.GetDirection(context.Mode),
            normalized.PersonsCount,
            queueEntry?.Id,
            command.Decision is VehicleEntryDecision.Allow
                ? VehicleVisitStatus.AwaitingControls
                : VehicleVisitStatus.EntryDenied,
            command.Decision is VehicleEntryDecision.Allow
                ? [new ControlResult(ControlKind.VehicleInspection, ControlStatus.Pending, null, null, null, null)]
                : [],
            enteredAt,
            null,
            1);
        var stored = new StoredVehicleVisit(
            vehicle,
            visit,
            queueEntry,
            idempotencyKey,
            CreateFingerprint(normalized, command.Decision, context));
        var persistence = await visitRepository.AddOrGetExistingAsync(stored, cancellationToken);

        if (!string.Equals(
                persistence.StoredVisit.RequestFingerprint,
                stored.RequestFingerprint,
                StringComparison.Ordinal))
        {
            return OperationResult<CreatedVehicleEntry>.Failure(
                VehicleEntryErrorCodes.IdempotencyConflict,
                "The idempotency key has already been used for another request.");
        }

        if (persistence.WasAdded)
        {
            await auditSink.AppendAsync(
                new AuditEvent(
                    Guid.NewGuid(),
                    "VehicleEntry",
                    user.Id,
                    NormalizeDeviceContext(deviceContext),
                    clock.UtcNow,
                    "VehicleVisit",
                    visit.Id.ToString("D"),
                    command.Decision.ToString(),
                    null,
                    SerializeForAudit(vehicle, visit, queueEntry),
                    "Success"),
                cancellationToken);
        }

        var result = persistence.StoredVisit;
        return OperationResult<CreatedVehicleEntry>.Success(
            new CreatedVehicleEntry(
                result.Vehicle,
                result.Visit,
                result.QueueEntry,
                persistence.WasAdded));
    }

    private async Task<OperationResult<SelectedOperatingContext>> GetEntryContextAsync(
        User user,
        CancellationToken cancellationToken)
    {
        if (!accessControl.HasPermission(user, Permission.ProcessVehicle))
        {
            return OperationResult<SelectedOperatingContext>.Failure(
                VehicleEntryErrorCodes.Forbidden,
                "Vehicle processing permission is required.");
        }

        var selected = await operatingContextService.GetCurrentAsync(user, cancellationToken);
        if (selected is null)
        {
            return OperationResult<SelectedOperatingContext>.Failure(
                VehicleEntryErrorCodes.OperatingContextRequired,
                "A valid operating context is required.");
        }

        if (!validationService.SupportsEntry(selected.Context.Mode))
        {
            return OperationResult<SelectedOperatingContext>.Failure(
                VehicleEntryErrorCodes.UnsupportedGateMode,
                "The selected gate mode does not receive vehicles onto the checkpoint territory.");
        }

        return OperationResult<SelectedOperatingContext>.Success(selected);
    }

    private static string CreateFingerprint(
        NormalizedVehicleEntry entry,
        VehicleEntryDecision decision,
        Domain.Models.OperatingContext context)
    {
        var source = string.Join(
            '\u001f',
            context.CheckpointId,
            context.GateId,
            entry.PlateNumber,
            entry.Brand,
            entry.Type,
            entry.IsServiceVehicle,
            entry.PersonsCount,
            string.Join('\u001e', entry.TrailerPlateNumbers),
            entry.PhotoReference,
            decision);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)));
    }

    private static string SerializeForAudit(
        Vehicle vehicle,
        VehicleVisit visit,
        QueueEntry? queueEntry) =>
        JsonSerializer.Serialize(new
        {
            visit.Id,
            vehicle.PlateNumber,
            vehicle.IsServiceVehicle,
            TrailerCount = vehicle.Trailers.Count,
            visit.PersonsCount,
            visit.CheckpointId,
            visit.GateId,
            visit.GateMode,
            visit.Direction,
            visit.Status,
            QueueEntryId = queueEntry?.Id
        });

    private static string NormalizeDeviceContext(string deviceContext) =>
        string.IsNullOrWhiteSpace(deviceContext)
            ? "unknown"
            : deviceContext[..Math.Min(deviceContext.Length, 512)];
}
