using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

/// <summary>Host-attributed, detached JSON data; contains no plugin objects or delegates.</summary>
public sealed record QuantumExtensionContribution(
    string PluginId,
    string LocalId,
    int Order,
    JsonElement Value);
