using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

public static class QuantumPluginInstallationExtensions
{
    public const string InstallMarketplaceRpcName = "quantum.plugins.installmarketplace";
    public const string InstallLocalRpcName = "quantum.plugins.installlocal";
    public const string GetPluginInstallStatusRpcName = "quantum.plugins.installstatus";

    /// <summary>Requests user approval to install the compatible Marketplace release.</summary>
    public static Task<Result<QuantumPluginInstallReceipt>> InstallMarketplacePluginAsync(
        this IRpcInvoker invoker, string pluginId, CancellationToken cancellationToken = default)
    {
        return invoker.InvokeAsync<QuantumPluginInstallReceipt>(InstallMarketplaceRpcName,
                new QuantumInstallMarketplacePluginRequest(pluginId), Context.Empty, cancellationToken);
    }

    /// <summary>Requests user approval to install a ZIP package supplied as bytes.</summary>
    public static Task<Result<QuantumPluginInstallReceipt>> InstallLocalPluginAsync(
        this IRpcInvoker invoker, string archiveFileName, byte[] archive,
        CancellationToken cancellationToken = default)
    {
        return invoker.InvokeAsync<QuantumPluginInstallReceipt>(InstallLocalRpcName,
                new QuantumInstallLocalPluginRequest(archiveFileName, archive), Context.Empty, cancellationToken);
    }

    /// <summary>Checks an approved installation after the request returns or the caller reloads.</summary>
    public static Task<Result<QuantumPluginInstallReceipt>> GetPluginInstallStatusAsync(
        this IRpcInvoker invoker, string operationId, CancellationToken cancellationToken = default)
    {
        return invoker.InvokeAsync<QuantumPluginInstallReceipt>(GetPluginInstallStatusRpcName,
                new QuantumPluginInstallStatusRequest(operationId), Context.Empty, cancellationToken);
    }
}
