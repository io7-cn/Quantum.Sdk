namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Adds a complete alternative invocation name for an RPC method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class RpcInvocationAliasAttribute : Attribute
{
    public RpcInvocationAliasAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public string Name { get; }
}
