using ETicket.Domain.Models;

namespace ETicket.Application.Ports;

public interface IAuditSink
{
    Task AppendAsync(AuditEvent auditEvent, CancellationToken cancellationToken);
}
