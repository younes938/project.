using ETicket.Application.Ports;
using ETicket.Domain.Models;

namespace ETicket.Api.Operations.Persistence;

public sealed class SqlServerAuditSink(OperationsDbContext db) : IAuditSink
{
    public async Task AppendAsync(AuditEvent item, CancellationToken cancellationToken)
    {
        db.AuditRecords.Add(new AuditRecordEntity
        {
            Id = item.Id, EventType = item.EventType, UserId = item.UserId,
            DeviceContext = item.DeviceContext, Timestamp = item.Timestamp,
            EntityType = item.EntityType, EntityId = item.EntityId, Action = item.Action,
            PreviousValueJson = item.PreviousValueJson, NewValueJson = item.NewValueJson,
            Result = item.Result
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
