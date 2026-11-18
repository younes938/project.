using System.Globalization;
using System.Security.Claims;
using ETicket.Domain.Enums;
using ETicket.Domain.Models;

namespace ETicket.Api.Security;

public static class UserPrincipalFactory
{
    public static ClaimsPrincipal Create(User user, AuthenticationMethod authenticationMethod)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(
                ETicketClaimTypes.AuthenticationMethod,
                authenticationMethod.ToString().ToLowerInvariant())
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Code)));
        claims.AddRange(
            user.Roles
                .SelectMany(role => role.Permissions)
                .Distinct()
                .Select(permission =>
                    new Claim(ETicketClaimTypes.Permission, PermissionClaims.FromPermission(permission))));
        claims.AddRange(
            user.VisibilityZones
                .SelectMany(zone => zone.CheckpointIds)
                .Distinct()
                .Select(checkpointId =>
                    new Claim(
                        ETicketClaimTypes.Checkpoint,
                        checkpointId.ToString("D", CultureInfo.InvariantCulture))));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "ETicket.Session"));
    }
}
