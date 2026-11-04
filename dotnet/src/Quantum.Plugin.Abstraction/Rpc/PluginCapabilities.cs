namespace Quantum.Plugin.Abstraction;

public sealed class PluginCapabilities
{
    public PluginCapabilities(
        IEnumerable<string>? provides = null,
        IEnumerable<string>? requires = null,
        IEnumerable<string>? integrates = null)
    {
        Provides = Validate(provides);
        Requires = Validate(requires);
        Integrates = Validate(integrates);
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (Provides.Concat(Requires).Concat(Integrates).Any(name => !names.Add(name)))
        {
            throw new ArgumentException("A capability may appear only once in a plugin manifest.");
        }
    }

    public IReadOnlyList<string> Provides { get; }

    public IReadOnlyList<string> Requires { get; }

    public IReadOnlyList<string> Integrates { get; }

    private static IReadOnlyList<string> Validate(IEnumerable<string>? names)
    {
        string[] values = (names ?? []).ToArray();
        foreach (string? name in values)
        {
            if (name is null || !name.Contains('.') || name.StartsWith("quantum.", StringComparison.Ordinal)
                || name.Split('.').Any(segment => segment.Length == 0
                    || segment.Any(character => character is not (>= 'a' and <= 'z'
                        or >= '0' and <= '9' or '-' or '_'))))
            {
                throw new ArgumentException($"Invalid canonical RPC capability name '{name}'.");
            }
        }

        return Array.AsReadOnly(values);
    }
}
