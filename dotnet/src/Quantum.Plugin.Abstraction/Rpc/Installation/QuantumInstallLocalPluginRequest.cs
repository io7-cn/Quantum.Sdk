namespace Quantum.Plugin.Abstraction;

public sealed record QuantumInstallLocalPluginRequest(string ArchiveFileName, byte[] ArchiveBase64);
