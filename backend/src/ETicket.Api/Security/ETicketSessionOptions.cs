namespace ETicket.Api.Security;

public sealed class ETicketSessionOptions
{
    public const string SectionName = "ETicket:Authentication";

    public int IdleTimeoutMinutes { get; init; } = 15;

    public bool EnableMockSignIn { get; init; }
}
