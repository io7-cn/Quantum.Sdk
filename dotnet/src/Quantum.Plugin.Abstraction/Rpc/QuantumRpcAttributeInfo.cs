namespace Quantum.Plugin.Abstraction;

public sealed record QuantumRpcAttributeInfo(
    string Type,
    IReadOnlyList<QuantumRpcAttributeArgumentInfo> ConstructorArguments,
    IReadOnlyDictionary<string, QuantumRpcAttributeArgumentInfo> NamedArguments);
