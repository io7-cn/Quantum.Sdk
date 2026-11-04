using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Represents one topic subscription.
/// </summary>
public interface IQuantumSubscription : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the normalized topic name.
    /// </summary>
    QuantumTopic Topic { get; }
}
