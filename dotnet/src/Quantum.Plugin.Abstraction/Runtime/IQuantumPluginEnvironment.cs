namespace Quantum.Plugin.Abstraction;

public interface IQuantumPluginEnvironment
{
    IReadOnlyList<QuantumPluginInfo> LoadedPlugins { get; }

    bool IsPluginLoaded(string pluginId);
}

public sealed record QuantumPluginInfo
{
    public QuantumPluginInfo(string id, string version)
    {
        Id = PluginId.Of(id);
        Version = SemanticVersion.Of(version);
    }

    public string Id { get; }

    public string Version { get; }
}
