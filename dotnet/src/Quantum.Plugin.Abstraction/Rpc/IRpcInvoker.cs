using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Invokes an RPC service implemented by any currently available Quantum plugin.
/// Payloads, contexts, and results cross a serialization boundary even for in-process
/// .NET plugins so that no plugin-owned CLR object escapes its runtime.
/// </summary>
public interface IRpcInvoker
{
    Task<Result<TResponse>> InvokeAsync<TResponse>(
        string rpcName,
        object payload,
        Context context,
        CancellationToken cancellationToken = default);

    Task<Result> InvokeAsync(
        string rpcName,
        object payload,
        Context context,
        CancellationToken cancellationToken = default);
}
