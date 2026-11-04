using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

public static class QuantumRpcCatalogExtensions
{
    public const string CatalogRpcName = "quantum.rpc.catalog";

    public static Task<Result<QuantumRpcCatalog>> GetCatalogAsync(
        this IRpcInvoker invoker,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoker);
        return invoker.InvokeAsync<QuantumRpcCatalog>(
            CatalogRpcName,
            new { },
            Context.Empty,
            cancellationToken);
    }
}
