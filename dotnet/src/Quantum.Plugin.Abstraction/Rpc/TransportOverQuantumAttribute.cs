using NOF.Contract;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Declares that an RPC service is transported through the Quantum plugin host.
/// NOF generates the protocol-neutral client interface, while Quantum supplies the
/// name-based transport through <see cref="IRpcInvoker"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class TransportOverQuantumAttribute : TransportOverAttribute;
