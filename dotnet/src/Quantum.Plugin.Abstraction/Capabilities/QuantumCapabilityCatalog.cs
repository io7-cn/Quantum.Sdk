using NOF.Contract;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quantum.Plugin.Abstraction;

[JsonConverter(typeof(JsonStringEnumConverter<QuantumCapabilityKind>))]
public enum QuantumCapabilityKind
{
    [JsonStringEnumMemberName("request")]
    Request,
    [JsonStringEnumMemberName("notify")]
    Notify
}

public sealed record QuantumCapabilityDescription(
    string Name,
    QuantumCapabilityKind Kind,
    int ContractVersion,
    string? Description,
    IReadOnlyList<string> ProviderPluginIds,
    JsonElement? RequestSchema,
    JsonElement? ResponseSchema,
    JsonElement? MessageSchema)
{
    public IReadOnlyList<QuantumCapabilityAttributeInfo> Attributes { get; init; } = [];
    public IReadOnlyList<QuantumCapabilityAttributeInfo> ParameterAttributes { get; init; } = [];
    public IReadOnlyList<QuantumCapabilityAttributeInfo> ReturnAttributes { get; init; } = [];
    public QuantumDispatch Dispatch { get; init; }
    public bool UniqueOwner { get; init; }
    public bool AllowStop { get; init; }
    public bool RequireSingleProvider { get; init; }
}

public sealed record QuantumCapabilityAttributeInfo(
    string Type,
    IReadOnlyList<QuantumCapabilityAttributeArgumentInfo> ConstructorArguments,
    IReadOnlyDictionary<string, QuantumCapabilityAttributeArgumentInfo> NamedArguments);

public sealed record QuantumCapabilityAttributeArgumentInfo(string Type, JsonElement Value);

public sealed record QuantumCapabilityCatalog(int SchemaVersion, IReadOnlyList<QuantumCapabilityDescription> Capabilities)
{
    public const string CapabilityName = "quantum.capabilities.list";
}

public static class QuantumCapabilityCatalogExtensions
{
    public static async Task<Result<QuantumCapabilityCatalog>> DiscoverCapabilitiesAsync(
        this IQuantumRequester requester, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requester);
        var report = await requester.RequestAsync<QuantumCapabilityCatalog>(
            QuantumCapabilityCatalog.CapabilityName, new { }, Context.Empty, cancellationToken).ConfigureAwait(false);
        if (!report.IsSuccess)
        {
            return Result<QuantumCapabilityCatalog>.From(report);
        }

        var answer = report.Value!.Outcomes.Count == 1 ? report.Value.Outcomes[0] : null;
        return answer is { Status: QuantumRequestStatus.Handled, Value: { } catalog }
            ? Result.Success(catalog)
            : Result<QuantumCapabilityCatalog>.From(Result.Fail(
                answer?.ErrorCode ?? "capability_catalog_unavailable",
                answer?.Message ?? "The capability catalog is unavailable.", answer?.Extra));
    }
}
