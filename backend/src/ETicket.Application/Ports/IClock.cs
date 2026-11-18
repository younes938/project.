namespace ETicket.Application.Ports;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
