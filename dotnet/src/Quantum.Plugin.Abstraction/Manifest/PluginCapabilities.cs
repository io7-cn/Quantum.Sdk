using System.Collections.ObjectModel;
using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

public sealed class PluginCapabilities
{
    public PluginCapabilities(
        IEnumerable<string>? provides = null,
        IEnumerable<string>? requires = null,
        IEnumerable<string>? integrates = null,
        IReadOnlyDictionary<string, QuantumNotificationContract>? notifications = null)
    {
        Provides = Validate(provides);
        Requires = Validate(requires);
        Integrates = Validate(integrates);
        Notifications = new ReadOnlyDictionary<string, QuantumNotificationContract>(
            (notifications ?? new Dictionary<string, QuantumNotificationContract>()).ToDictionary(pair => pair.Key,
                pair => ValidateNotification(pair.Key, pair.Value), StringComparer.Ordinal));

        var names = new HashSet<string>(StringComparer.Ordinal);
        if (Provides.Concat(Requires).Concat(Integrates).Any(name => !names.Add(name)))
        {
            throw new ArgumentException("A capability may appear only once in a plugin manifest.");
        }

        var kinds = Provides.ToDictionary(name => name, _ => QuantumCapabilityKind.Request, StringComparer.Ordinal);
        foreach (var name in Notifications.Keys)
        {
            setKind(name, QuantumCapabilityKind.Notify);
        }

        ProvidedKinds = new ReadOnlyDictionary<string, QuantumCapabilityKind>(kinds);

        void setKind(string name, QuantumCapabilityKind kind)
        {
            if (!kinds.TryGetValue(name, out var existing) || existing != QuantumCapabilityKind.Request)
            {
                throw new ArgumentException($"Capability '{name}' must appear in provides and declare only one kind.");
            }

            kinds[name] = kind;
        }
    }

    public IReadOnlyList<string> Provides { get; }

    public IReadOnlyList<string> Requires { get; }

    public IReadOnlyList<string> Integrates { get; }

    public IReadOnlyDictionary<string, QuantumNotificationContract> Notifications { get; }

    public IReadOnlyDictionary<string, QuantumCapabilityKind> ProvidedKinds { get; }

    private static QuantumNotificationContract ValidateNotification(string name, QuantumNotificationContract contract)
    {
        if (contract is null || contract.MessageSchema.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException($"Notification '{name}' must declare a JSON Schema object.");
        }

        if (!Enum.IsDefined(contract.Dispatch) || contract.StopObservers is null
            || contract.Dispatch == QuantumDispatch.All && contract.StopObservers.Count != 0)
        {
            throw new ArgumentException($"Notification '{name}' has invalid propagation settings.");
        }

        var observers = contract.StopObservers.Select(value => (string)PluginId.Of(value))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        return contract with { MessageSchema = contract.MessageSchema.Clone(), StopObservers = Array.AsReadOnly(observers) };
    }

    private static IReadOnlyList<string> Validate(IEnumerable<string>? names)
    {
        var values = (names ?? []).ToArray();
        foreach (var name in values)
        {
            if (!CapabilityName.IsPluginCapability(name))
            {
                throw new ArgumentException($"Invalid plugin capability name '{name}'.");
            }
        }

        return Array.AsReadOnly(values);
    }
}
