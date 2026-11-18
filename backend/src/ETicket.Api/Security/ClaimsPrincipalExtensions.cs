using System.Security.Claims;

namespace ETicket.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static bool CanAccessCheckpoint(this ClaimsPrincipal principal, Guid checkpointId) =>
        principal.HasClaim(
            ETicketClaimTypes.Checkpoint,
            checkpointId.ToString("D"));
}
