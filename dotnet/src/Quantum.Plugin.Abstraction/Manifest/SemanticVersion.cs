using NOF.Domain;
using System.Globalization;
using System.Numerics;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Identifies a Semantic Versioning 2.0.0 version and exposes its precedence semantics.
/// </summary>
public static class SemanticVersion
{
    public static IComparer<string> PrecedenceComparer { get; } = Comparer<string>.Create(Compare);

    public static string Of(string value)
    {
        Validate(value);
        return value;
    }

    public static int Compare(string? left, string? right)
        => Compare(Components(left), Components(right));

    public static SemanticVersionComponents Components(string? value)
        => TryGetComponents(value, out var components)
            ? components
            : throw new DomainValidationException($"'{value}' is not a valid Semantic Versioning 2.0.0 version.");

    public static void Validate(string value)
    {
        if (!TryGetComponents(value, out _))
        {
            throw new DomainValidationException(
                $"'{value}' is not a valid Semantic Versioning 2.0.0 version.");
        }
    }

    private static bool TryGetComponents(
        string? value,
        out SemanticVersionComponents components)
    {
        components = default;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var buildSeparator = value.IndexOf('+', StringComparison.Ordinal);
        var versionWithoutBuild = buildSeparator < 0 ? value : value[..buildSeparator];
        var buildValue = buildSeparator < 0 ? null : value[(buildSeparator + 1)..];
        if (buildSeparator >= 0
            && (value.IndexOf('+', buildSeparator + 1) >= 0 || string.IsNullOrEmpty(buildValue)))
        {
            return false;
        }

        var preReleaseSeparator = versionWithoutBuild.IndexOf('-', StringComparison.Ordinal);
        var coreValue = preReleaseSeparator < 0
            ? versionWithoutBuild
            : versionWithoutBuild[..preReleaseSeparator];
        var preReleaseValue = preReleaseSeparator < 0
            ? null
            : versionWithoutBuild[(preReleaseSeparator + 1)..];
        if (preReleaseSeparator >= 0 && string.IsNullOrEmpty(preReleaseValue))
        {
            return false;
        }

        var coreIdentifiers = coreValue.Split('.', StringSplitOptions.None);
        if (coreIdentifiers.Length != 3
            || !TryParseCoreIdentifier(coreIdentifiers[0], out var major)
            || !TryParseCoreIdentifier(coreIdentifiers[1], out var minor)
            || !TryParseCoreIdentifier(coreIdentifiers[2], out var patch))
        {
            return false;
        }

        var preReleaseIdentifiers = SplitIdentifiers(preReleaseValue);
        var buildMetadataIdentifiers = SplitIdentifiers(buildValue);
        if (preReleaseIdentifiers.Any(static identifier =>
                !IsValidIdentifier(identifier)
                || (IsNumericIdentifier(identifier) && HasLeadingZero(identifier)))
            || buildMetadataIdentifiers.Any(static identifier => !IsValidIdentifier(identifier)))
        {
            return false;
        }

        components = new SemanticVersionComponents(
            major,
            minor,
            patch,
            preReleaseIdentifiers,
            buildMetadataIdentifiers);
        return true;
    }

    private static int Compare(
        SemanticVersionComponents left,
        SemanticVersionComponents right)
    {
        var comparison = left.Major.CompareTo(right.Major);
        if (comparison == 0)
        {
            comparison = left.Minor.CompareTo(right.Minor);
        }

        if (comparison == 0)
        {
            comparison = left.Patch.CompareTo(right.Patch);
        }

        if (comparison != 0)
        {
            return comparison;
        }

        if (left.PreReleaseIdentifiers.Length == 0 || right.PreReleaseIdentifiers.Length == 0)
        {
            return left.PreReleaseIdentifiers.Length == right.PreReleaseIdentifiers.Length
                ? 0
                : left.PreReleaseIdentifiers.Length == 0 ? 1 : -1;
        }

        for (var index = 0;
             index < Math.Min(
                 left.PreReleaseIdentifiers.Length,
                 right.PreReleaseIdentifiers.Length);
             index++)
        {
            comparison = ComparePreReleaseIdentifier(
                left.PreReleaseIdentifiers[index],
                right.PreReleaseIdentifiers[index]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        return left.PreReleaseIdentifiers.Length.CompareTo(right.PreReleaseIdentifiers.Length);
    }

    private static bool TryParseCoreIdentifier(string value, out BigInteger number)
    {
        number = default;
        return value.Length > 0
            && value.All(char.IsAsciiDigit)
            && BigInteger.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number)
            && !HasLeadingZero(value);
    }

    private static string[] SplitIdentifiers(string? value)
        => value is null ? [] : value.Split('.', StringSplitOptions.None);

    private static bool IsValidIdentifier(string value)
        => value.Length > 0
            && value.All(static character =>
                char.IsAsciiLetterOrDigit(character) || character == '-');

    private static bool IsNumericIdentifier(string value)
        => value.All(char.IsAsciiDigit);

    private static bool HasLeadingZero(string value)
        => value.Length > 1 && value[0] == '0';

    private static int ComparePreReleaseIdentifier(string left, string right)
    {
        var leftNumeric = IsNumericIdentifier(left);
        var rightNumeric = IsNumericIdentifier(right);
        if (leftNumeric != rightNumeric)
        {
            return leftNumeric ? -1 : 1;
        }

        if (!leftNumeric)
        {
            return string.Compare(left, right, StringComparison.Ordinal);
        }

        var comparison = left.Length.CompareTo(right.Length);
        return comparison == 0
            ? string.Compare(left, right, StringComparison.Ordinal)
            : comparison;
    }

    public readonly record struct SemanticVersionComponents(
        BigInteger Major,
        BigInteger Minor,
        BigInteger Patch,
        string[] PreReleaseIdentifiers,
        string[] BuildMetadataIdentifiers)
    {
        public bool IsPreRelease => PreReleaseIdentifiers.Length > 0;

        public string? PreRelease => IsPreRelease ? string.Join('.', PreReleaseIdentifiers) : null;

        public string? BuildMetadata => BuildMetadataIdentifiers.Length > 0 ? string.Join('.', BuildMetadataIdentifiers) : null;
    }
}
