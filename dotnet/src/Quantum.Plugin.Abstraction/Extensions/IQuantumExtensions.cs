using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

/// <summary>Runtime-scoped, type-independent extension points and JSON contributions.</summary>
public interface IQuantumExtensions
{
    /// <summary>Defines a point owned by this plugin. Returns a handle that removes the definition.</summary>
    IDisposable Define(string localId, int contractVersion);

    /// <summary>Contributes JSON to a point, including a point whose owner is not currently active.</summary>
    IQuantumContribution Register(string pointId, int contractVersion, string localId, JsonElement value, int order = 0);

    /// <summary>Observes an initial snapshot and subsequent ordered snapshots without a subscription gap.</summary>
    IDisposable Observe(string pointId, int contractVersion, Action<QuantumExtensionSnapshot> observer);
}
