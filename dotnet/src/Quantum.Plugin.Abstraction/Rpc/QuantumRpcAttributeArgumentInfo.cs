using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

public sealed record QuantumRpcAttributeArgumentInfo(string Type, JsonElement Value);
