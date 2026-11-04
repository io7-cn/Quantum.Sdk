using System.Text.RegularExpressions;

namespace Quantum.Plugin.Abstraction;

/// <summary>Validates the single name used for a Quantum capability.</summary>
public static partial class CapabilityName
{
    public static bool IsValid(string? name)
        => name is { Length: <= 200 } && Pattern().IsMatch(name);

    public static bool IsPluginCapability(string? name)
        => IsValid(name) && !name!.StartsWith("quantum.", StringComparison.Ordinal);

    [GeneratedRegex(@"\A[a-z0-9_-]+(?:\.[a-z0-9_-]+)+\z", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
