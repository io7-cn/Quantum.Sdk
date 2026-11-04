namespace Quantum.Plugin.Abstraction;

/// <summary>
/// A serializable snapshot of every RPC method currently available through Quantum.
/// </summary>
public sealed record QuantumRpcCatalog(
    int SchemaVersion,
    string CatalogRpcName,
    IReadOnlyList<QuantumRpcServiceInfo> Services);
