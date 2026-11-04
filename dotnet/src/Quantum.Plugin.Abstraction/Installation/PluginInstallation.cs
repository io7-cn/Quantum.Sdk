using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

public sealed record QuantumInstallMarketplacePluginRequest(string PluginId);

public sealed record QuantumInstallLocalPluginRequest(string ArchiveFileName, byte[] ArchiveBase64);

public sealed record QuantumPluginInstallStatusRequest(string OperationId);

public sealed record QuantumPluginInstallReceipt(string OperationId, string Status, string? Message);

public static class QuantumPluginInstallationExtensions
{
    public const string InstallMarketplaceCapabilityName = "quantum.plugins.installmarketplace";
    public const string InstallLocalCapabilityName = "quantum.plugins.installlocal";
    public const string GetPluginInstallStatusCapabilityName = "quantum.plugins.installstatus";

    /// <summary>Requests user approval to install the compatible Marketplace release.</summary>
    public static Task<Result<QuantumPluginInstallReceipt>> InstallMarketplacePluginAsync(
        this IQuantumRequester invoker, string pluginId, CancellationToken cancellationToken = default)
        => invoker.RequestSingleAsync<QuantumPluginInstallReceipt>(InstallMarketplaceCapabilityName,
            new QuantumInstallMarketplacePluginRequest(pluginId), Context.Empty, cancellationToken);

    /// <summary>Requests user approval to install a ZIP package supplied as bytes.</summary>
    public static Task<Result<QuantumPluginInstallReceipt>> InstallLocalPluginAsync(
        this IQuantumRequester invoker, string archiveFileName, byte[] archive,
        CancellationToken cancellationToken = default)
        => invoker.RequestSingleAsync<QuantumPluginInstallReceipt>(InstallLocalCapabilityName,
            new QuantumInstallLocalPluginRequest(archiveFileName, archive), Context.Empty, cancellationToken);

    /// <summary>Checks an approved installation after the request returns or the caller reloads.</summary>
    public static Task<Result<QuantumPluginInstallReceipt>> GetPluginInstallStatusAsync(
        this IQuantumRequester invoker, string operationId, CancellationToken cancellationToken = default)
        => invoker.RequestSingleAsync<QuantumPluginInstallReceipt>(GetPluginInstallStatusCapabilityName,
            new QuantumPluginInstallStatusRequest(operationId), Context.Empty, cancellationToken);
}
