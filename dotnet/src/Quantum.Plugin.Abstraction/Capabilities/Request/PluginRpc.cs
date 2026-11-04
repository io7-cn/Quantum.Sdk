using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Declares that an RPC service is transported through the Quantum plugin host.
/// NOF generates the protocol-neutral client interface, while Quantum supplies the
/// name-based transport through <see cref="IQuantumRequester"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class TransportOverQuantumAttribute : TransportOverAttribute;

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
