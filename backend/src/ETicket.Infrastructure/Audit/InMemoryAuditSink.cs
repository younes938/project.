using System.Collections.Concurrent;
using ETicket.Application.Ports;
using ETicket.Domain.Models;

namespace ETicket.Infrastructure.Audit;

public sealed class InMemoryAuditSink : IAuditSink
{
    private readonly ConcurrentQueue<AuditEvent> events = new();

    public IReadOnlyCollection<AuditEvent> Events => events.ToArray();

    public Task AppendAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        events.Enqueue(auditEvent);
        return Task.CompletedTask;
    }
}
