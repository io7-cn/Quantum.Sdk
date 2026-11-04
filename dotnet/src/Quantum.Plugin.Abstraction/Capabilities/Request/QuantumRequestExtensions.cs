using NOF.Contract;
using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

public static class QuantumRequestExtensions
{
    /// <summary>Projects a request report when the caller requires exactly one handled outcome with a value.</summary>
    public static async Task<Result<T>> RequestSingleAsync<T>(
        this IQuantumRequester requester, string name, object payload, Context context,
        CancellationToken cancellationToken = default)
    {
        var report = await requester.RequestAsync<T>(name, payload, context, cancellationToken).ConfigureAwait(false);
        var answer = SingleOutcome(name, report);
        if (!answer.IsSuccess)
        {
            return Result<T>.From(answer);
        }

        return answer.Value.HasValue && answer.Value.Value is { } value
            ? Result.Success(value, answer.Value.Extra)
            : Result<T>.From(Result.Fail("rpc_invalid_result", $"'{name}' returned no value."));
    }

    public static async Task<Result> RequestSingleAsync(
        this IQuantumRequester requester, string name, object payload, Context context,
        CancellationToken cancellationToken = default)
    {
        var report = await requester.RequestAsync<JsonElement?>(name, payload, context, cancellationToken).ConfigureAwait(false);
        var answer = SingleOutcome(name, report);
        return answer.IsSuccess
            ? Result.Success(answer.Value.Extra)
            : (Result)Result.Fail(answer.ErrorCode, answer.Message, answer.Extra);
    }

    private static Result<QuantumRequestOutcome<T>> SingleOutcome<T>(string name, Result<QuantumRequestReport<T>> report)
    {
        if (!report.IsSuccess)
        {
            return Result<QuantumRequestOutcome<T>>.From(report);
        }

        var outcomes = report.Value.Outcomes.Where(outcome => outcome.Status != QuantumRequestStatus.Pass).ToArray();
        return outcomes switch
        {
            [{ Status: QuantumRequestStatus.Handled } answer] => Result.Success(answer),
            [var failure] => Result.Fail(failure.ErrorCode!, failure.Message!, failure.Extra),
            [] => Result.Fail("rpc_not_found", $"No provider handled '{name}'."),
            _ => Result.Fail("capability_multiple_outcomes", $"'{name}' did not produce a single outcome.")
        };
    }
}
