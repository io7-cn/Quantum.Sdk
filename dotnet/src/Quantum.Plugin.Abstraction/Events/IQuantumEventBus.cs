using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Provides ROS-style publish/subscribe messaging between Quantum plugins through named topics.
/// </summary>
/// <remarks>
/// The bus is local to the current Quantum host. Messages are delivered to the subscriptions that
/// are active when <see cref="IQuantumPublisher{TMessage}.PublishAsync"/> is called; messages are not
/// retained or replayed.
/// </remarks>
public interface IQuantumEventBus
{
    /// <summary>
    /// Creates a publisher for <paramref name="topic"/>.
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TMessage"/> controls plugin-side serialization only. Topic routing does
    /// not use or transmit CLR type identity.
    /// </remarks>
    IQuantumPublisher<TMessage> CreatePublisher<TMessage>(QuantumTopic topic);

    /// <summary>
    /// Subscribes to <paramref name="topic"/> until the returned handle is disposed.
    /// </summary>
    IQuantumSubscription Subscribe(
        QuantumTopic topic,
        Func<QuantumEvent, CancellationToken, Task> handler);
}
