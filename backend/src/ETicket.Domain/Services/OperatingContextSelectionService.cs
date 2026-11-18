using ETicket.Domain.Models;

namespace ETicket.Domain.Services;

public enum OperatingContextValidationError
{
    None,
    UserInactive,
    CheckpointInactive,
    CheckpointNotVisible,
    GateInactive,
    GateDoesNotBelongToCheckpoint
}

public sealed class OperatingContextSelectionService(AccessControlService accessControl)
{
    public OperatingContextValidationError Validate(
        User user,
        Checkpoint checkpoint,
        Gate gate)
    {
        if (!user.IsActive)
        {
            return OperatingContextValidationError.UserInactive;
        }

        if (!checkpoint.IsActive)
        {
            return OperatingContextValidationError.CheckpointInactive;
        }

        if (!accessControl.CanAccessCheckpoint(user, checkpoint.Id))
        {
            return OperatingContextValidationError.CheckpointNotVisible;
        }

        if (!gate.IsActive)
        {
            return OperatingContextValidationError.GateInactive;
        }

        return gate.CheckpointId == checkpoint.Id
            ? OperatingContextValidationError.None
            : OperatingContextValidationError.GateDoesNotBelongToCheckpoint;
    }
}
