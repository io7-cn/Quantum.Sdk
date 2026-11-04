namespace Quantum.Plugin.Abstraction;

public sealed record QuantumPluginInstallReceipt(string OperationId, string Status, string? Message);
