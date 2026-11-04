using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Publishes messages of <typeparamref name="TMessage"/> to one topic.
/// </summary>
public interface IQuantumPublisher<in TMessage>
{
    /// <summary>
    /// Gets the normalized topic name.
    /// </summary>
    QuantumTopic Topic { get; }

    /// <summary>
    /// Publishes a message and waits for the current in-process subscribers to finish.
    /// </summary>
    ValueTask PublishAsync(TMessage message, CancellationToken cancellationToken = default);
}
