using System.Security.Claims;

namespace ETicket.Api.Security;

public sealed record SessionResponse(
    Guid Id,
    string DisplayName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    IReadOnlyCollection<Guid> CheckpointIds,
    string AuthenticationMethod,
    int IdleTimeoutMinutes)
{
    public static SessionResponse FromPrincipal(ClaimsPrincipal principal, int idleTimeoutMinutes)
    {
        var id = Guid.Parse(
            principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("Authenticated principal has no user ID."));

        return new SessionResponse(
            id,
            principal.Identity?.Name ?? string.Empty,
            principal.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToArray(),
            principal.FindAll(ETicketClaimTypes.Permission).Select(claim => claim.Value).Distinct().ToArray(),
            principal.FindAll(ETicketClaimTypes.Checkpoint).Select(claim => Guid.Parse(claim.Value)).Distinct().ToArray(),
            principal.FindFirstValue(ETicketClaimTypes.AuthenticationMethod) ?? "unknown",
            idleTimeoutMinutes);
    }
}
