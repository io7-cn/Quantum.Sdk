namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Replaces the default service or method component used to build a Quantum RPC name.
/// </summary>
[AttributeUsage(
    AttributeTargets.Interface | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = false)]
public sealed class RpcInvocationNameAttribute : Attribute
{
    public RpcInvocationNameAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }
}
