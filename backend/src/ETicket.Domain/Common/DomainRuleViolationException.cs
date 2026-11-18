namespace ETicket.Domain.Common;

public sealed class DomainRuleViolationException : Exception
{
    public DomainRuleViolationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
