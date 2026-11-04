namespace Quantum.Plugin.Abstraction;

public interface IQuantumNotificationSubscription : IDisposable, IAsyncDisposable
{
    string CapabilityName { get; }
}
