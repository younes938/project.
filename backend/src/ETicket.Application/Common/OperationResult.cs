namespace ETicket.Application.Common;

public sealed record ApplicationError(string Code, string Message);

public sealed class OperationResult<T>
{
    private OperationResult(T? value, ApplicationError? error)
    {
        Value = value;
        Error = error;
    }

    public bool IsSuccess => Error is null;

    public T? Value { get; }

    public ApplicationError? Error { get; }

    public static OperationResult<T> Success(T value) => new(value, null);

    public static OperationResult<T> Failure(string code, string message) =>
        new(default, new ApplicationError(code, message));
}
