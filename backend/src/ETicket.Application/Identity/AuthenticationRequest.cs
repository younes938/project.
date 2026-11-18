using ETicket.Domain.Enums;

namespace ETicket.Application.Identity;

public sealed record AuthenticationRequest(
    string Subject,
    AuthenticationMethod Method);
