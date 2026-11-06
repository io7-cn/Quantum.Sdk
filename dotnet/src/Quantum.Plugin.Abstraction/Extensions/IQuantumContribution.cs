using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

/// <summary>Owns one contribution. Disposal is idempotent; updates after disposal are rejected.</summary>
public interface IQuantumContribution : IDisposable
{
    void Update(JsonElement value);
}
