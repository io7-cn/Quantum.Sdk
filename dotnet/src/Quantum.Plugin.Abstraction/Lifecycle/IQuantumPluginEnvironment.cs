namespace Quantum.Plugin.Abstraction;

public interface IQuantumPluginEnvironment
{
    IReadOnlyList<QuantumPluginInfo> LoadedPlugins { get; }

    bool IsPluginLoaded(PluginId pluginId);
}
