using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Reserved <see cref="Context"/> keys injected by the Quantum host for every RPC call.
/// </summary>
public static class QuantumRpcContextKeys
{
    public const string CallerPluginId = "quantum.rpc.callerPluginId";

    public const string CallerRuntimeId = "quantum.rpc.callerRuntimeId";

    public const string MobileDeviceId = "quantum.mobile.deviceId";
    public const string MobileRequestId = "quantum.mobile.requestId";
    public const string MobileViewId = "quantum.mobile.viewId";
}
