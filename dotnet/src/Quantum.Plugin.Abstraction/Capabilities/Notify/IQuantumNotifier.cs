namespace Quantum.Plugin.Abstraction;

/// <summary>Publishes declared notifications and observes JSON payloads by capability name.</summary>
public interface IQuantumNotifier
{
    ValueTask PublishAsync<TMessage>(
        string capabilityName, TMessage message, CancellationToken cancellationToken = default);

    IQuantumNotificationSubscription Subscribe(
        string capabilityName, Func<QuantumNotification, CancellationToken, Task> handler, int order = 0);

    IQuantumNotificationSubscription Observe(string capabilityName,
        Func<QuantumNotification, CancellationToken, Task<QuantumPropagation>> handler, int order = 0);
}
