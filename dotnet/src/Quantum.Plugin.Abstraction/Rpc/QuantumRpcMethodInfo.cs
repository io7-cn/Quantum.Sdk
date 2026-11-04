using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

public sealed record QuantumRpcMethodInfo(
    string QualifiedName,
    string CanonicalName,
    IReadOnlyList<string> Aliases,
    string Declaration,
    string MethodName,
    string? Description,
    string RequestType,
    string? ResponseType,
    bool ReturnsValue,
    JsonElement InputSchema,
    JsonElement OutputSchema,
    IReadOnlyList<QuantumRpcAttributeInfo> Attributes,
    IReadOnlyList<QuantumRpcAttributeInfo> ParameterAttributes,
    IReadOnlyList<QuantumRpcAttributeInfo> ReturnAttributes);
