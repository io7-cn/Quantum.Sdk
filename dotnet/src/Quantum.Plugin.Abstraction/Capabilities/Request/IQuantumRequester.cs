using NOF.Contract;
using System.Text.Json.Serialization;

namespace Quantum.Plugin.Abstraction;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class RequestCapabilityAttribute(string name, QuantumDispatch dispatch) : Attribute
{
    public string Name { get; } = CapabilityName.IsPluginCapability(name)
        ? name : throw new ArgumentException("Expected a plugin capability name.", nameof(name));

    public QuantumDispatch Dispatch { get; } = dispatch;

    public bool UniqueOwner { get; init; }

    public bool RequireSingleProvider { get; init; }

    public bool AllowStop { get; init; }

    public int Order { get; init; }
}

public static class QuantumRequestContext
{
    public const string Dispatched = "quantum.request.dispatched";
    public const string PassCode = "capability_pass";
}

[JsonConverter(typeof(JsonStringEnumConverter<QuantumRequestStatus>))]
public enum QuantumRequestStatus
{
    [JsonStringEnumMemberName("handled")]
    Handled,
    [JsonStringEnumMemberName("pass")]
    Pass,
    [JsonStringEnumMemberName("failed")]
    Failed,
    [JsonStringEnumMemberName("timedOut")]
    TimedOut
}

public sealed record QuantumRequestOutcome<T>(
    string ProviderPluginId, QuantumRequestStatus Status, T? Value, string? ErrorCode, string? Message)
{
    public bool HasValue { get; init; }

    public IDictionary<string, string>? Extra { get; init; }
}

public sealed record QuantumRequestReport<T>(IReadOnlyList<QuantumRequestOutcome<T>> Outcomes, bool Stopped);

public interface IQuantumRequester
{
    Task<Result<QuantumRequestReport<T>>> RequestAsync<T>(
        string capabilityName, object payload, Context context, CancellationToken cancellationToken = default);
}
