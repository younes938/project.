using System.Security.Claims;
using ETicket.Application.Ports;
using ETicket.Domain.Models;

namespace ETicket.Api.Security;

public static class AuthAudit
{
    public static Task WriteAsync(
        IAuditSink auditSink,
        IClock clock,
        HttpContext httpContext,
        string action,
        string result,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var deviceContext = httpContext.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(deviceContext))
        {
            deviceContext = "unknown";
        }

        deviceContext = deviceContext[..Math.Min(deviceContext.Length, 512)];

        var auditEvent = new AuditEvent(
            Guid.NewGuid(),
            "Authentication",
            userId ?? TryGetUserId(httpContext.User),
            deviceContext,
            clock.UtcNow,
            "Session",
            null,
            action,
            null,
            null,
            result);

        return auditSink.AppendAsync(auditEvent, cancellationToken);
    }

    private static Guid? TryGetUserId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : null;
}
