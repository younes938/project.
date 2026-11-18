using System.Text.Json;
using ETicket.Application.Common;
using ETicket.Application.Ports;
using ETicket.Domain.Models;
using ETicket.Domain.Services;

namespace ETicket.Application.OperatingContexts;

public sealed record AvailableCheckpoint(
    Checkpoint Checkpoint,
    IReadOnlyCollection<Gate> Gates);

public sealed record SelectedOperatingContext(
    Domain.Models.OperatingContext Context,
    Checkpoint Checkpoint,
    Gate Gate);

public static class OperatingContextErrorCodes
{
    public const string SelectionNotAvailable = "operating_context.selection_not_available";
    public const string GateCheckpointMismatch = "operating_context.gate_checkpoint_mismatch";
}

public sealed class OperatingContextService(
    ICheckpointCatalog checkpointCatalog,
    IOperatingContextRepository contextRepository,
    OperatingContextSelectionService selectionService,
    AccessControlService accessControl,
    IClock clock,
    IAuditSink auditSink)
{
    public async Task<IReadOnlyCollection<AvailableCheckpoint>> GetOptionsAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var checkpoints = await checkpointCatalog.ListCheckpointsAsync(cancellationToken);
        var gates = await checkpointCatalog.ListGatesAsync(cancellationToken);

        return checkpoints
            .Where(checkpoint =>
                checkpoint.IsActive && accessControl.CanAccessCheckpoint(user, checkpoint.Id))
            .OrderBy(checkpoint => checkpoint.Name, StringComparer.OrdinalIgnoreCase)
            .Select(checkpoint => new AvailableCheckpoint(
                checkpoint,
                gates
                    .Where(gate => gate.IsActive && gate.CheckpointId == checkpoint.Id)
                    .OrderBy(gate => gate.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray()))
            .ToArray();
    }

    public async Task<SelectedOperatingContext?> GetCurrentAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var current = await contextRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var checkpoint = await checkpointCatalog.GetCheckpointAsync(
            current.CheckpointId,
            cancellationToken);
        var gate = await checkpointCatalog.GetGateAsync(current.GateId, cancellationToken);

        if (checkpoint is null ||
            gate is null ||
            selectionService.Validate(user, checkpoint, gate) is not OperatingContextValidationError.None)
        {
            await contextRepository.DeleteByUserIdAsync(user.Id, cancellationToken);
            return null;
        }

        var validatedContext = current with { Mode = gate.Mode };
        return new SelectedOperatingContext(validatedContext, checkpoint, gate);
    }

    public async Task<OperationResult<SelectedOperatingContext>> SelectAsync(
        User user,
        Guid checkpointId,
        Guid gateId,
        string deviceContext,
        CancellationToken cancellationToken)
    {
        var checkpoint = await checkpointCatalog.GetCheckpointAsync(
            checkpointId,
            cancellationToken);
        var gate = await checkpointCatalog.GetGateAsync(gateId, cancellationToken);

        if (checkpoint is null || gate is null)
        {
            return OperationResult<SelectedOperatingContext>.Failure(
                OperatingContextErrorCodes.SelectionNotAvailable,
                "The selected operating context is not available.");
        }

        var validationError = selectionService.Validate(user, checkpoint, gate);
        if (validationError is not OperatingContextValidationError.None)
        {
            var errorCode = validationError is OperatingContextValidationError.GateDoesNotBelongToCheckpoint
                ? OperatingContextErrorCodes.GateCheckpointMismatch
                : OperatingContextErrorCodes.SelectionNotAvailable;

            return OperationResult<SelectedOperatingContext>.Failure(
                errorCode,
                "The selected operating context is not available.");
        }

        var previous = await contextRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (previous is not null &&
            previous.CheckpointId == checkpoint.Id &&
            previous.GateId == gate.Id &&
            previous.Mode == gate.Mode)
        {
            return OperationResult<SelectedOperatingContext>.Success(
                new SelectedOperatingContext(previous, checkpoint, gate));
        }

        var selected = new Domain.Models.OperatingContext(
            user.Id,
            checkpoint.Id,
            gate.Id,
            gate.Mode,
            clock.UtcNow);

        await contextRepository.UpsertAsync(selected, cancellationToken);
        await auditSink.AppendAsync(
            new AuditEvent(
                Guid.NewGuid(),
                "OperatingContext",
                user.Id,
                NormalizeDeviceContext(deviceContext),
                clock.UtcNow,
                "OperatingContext",
                user.Id.ToString("D"),
                "Select",
                Serialize(previous),
                Serialize(selected),
                "Success"),
            cancellationToken);

        return OperationResult<SelectedOperatingContext>.Success(
            new SelectedOperatingContext(selected, checkpoint, gate));
    }

    private static string? Serialize(Domain.Models.OperatingContext? context) =>
        context is null
            ? null
            : JsonSerializer.Serialize(new
            {
                context.CheckpointId,
                context.GateId,
                context.Mode
            });

    private static string NormalizeDeviceContext(string deviceContext)
    {
        if (string.IsNullOrWhiteSpace(deviceContext))
        {
            return "unknown";
        }

        return deviceContext[..Math.Min(deviceContext.Length, 512)];
    }
}
