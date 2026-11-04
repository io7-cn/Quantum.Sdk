using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

/// <summary>Declares the contract for a notification listed in the manifest's provides section.</summary>
public sealed record QuantumNotificationContract(string? Description, JsonElement MessageSchema)
{
    public QuantumDispatch Dispatch { get; init; } = QuantumDispatch.All;

    public IReadOnlyList<string> StopObservers { get; init; } = [];
}
