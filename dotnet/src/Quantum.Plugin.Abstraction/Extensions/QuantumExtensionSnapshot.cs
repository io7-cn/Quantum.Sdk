namespace Quantum.Plugin.Abstraction;

/// <summary>An immutable point snapshot. Revisions increase within the host session.</summary>
public sealed record QuantumExtensionSnapshot(
    string PointId,
    int ContractVersion,
    long Revision,
    bool IsAvailable,
    IReadOnlyList<QuantumExtensionContribution> Contributions);
