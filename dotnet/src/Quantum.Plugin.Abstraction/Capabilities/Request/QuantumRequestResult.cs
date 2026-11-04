using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

/// <summary>A NOF handler result that can answer, pass, or end an ordered request.</summary>
public sealed class QuantumRequestResult<T> : IResult<QuantumRequestResult<T>>
{
    private readonly IResult _result;

    private QuantumRequestResult(IResult result, bool stop = false)
    {
        _result = result;
        Stop = stop;
    }

    public bool IsSuccess => _result.IsSuccess;
    public string ErrorCode => _result.ErrorCode;
    public string Message => _result.Message;
    public T? Value => IsSuccess ? (T?)_result.Value : default;
    object? IResult.Value => _result.Value;
    public IDictionary<string, string> Extra => _result.Extra;
    public bool Stop { get; }

    public static QuantumRequestResult<T> Answer(T value) => new(Result.Success(value));

    public static QuantumRequestResult<T> AnswerAndStop(T value) => new(Result.Success(value), stop: true);

    public static QuantumRequestResult<T> Pass()
        => new(Result.Fail(QuantumRequestContext.PassCode, "The provider passed."));

    public static QuantumRequestResult<T> From(IResult other)
        => other as QuantumRequestResult<T> ?? new(Result<T>.From(other));
}
