namespace Quantum.Plugin.Abstraction;

public sealed record QuantumRpcServiceInfo(
    string PluginId,
    string ServiceName,
    string ServiceType,
    string? Description,
    IReadOnlyList<QuantumRpcAttributeInfo> Attributes,
    IReadOnlyList<QuantumRpcMethodInfo> Methods);
