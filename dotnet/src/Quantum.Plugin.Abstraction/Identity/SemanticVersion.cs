using NOF.Domain;
using System.Globalization;
using System.Numerics;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Identifies a Semantic Versioning 2.0.0 version and exposes its precedence semantics.
/// </summary>
public readonly partial struct SemanticVersion :
    IValueObject<string>,
    IComparable<SemanticVersion>
{
    public BigInteger Major => GetComponents().Major;

    public BigInteger Minor => GetComponents().Minor;

    public BigInteger Patch => GetComponents().Patch;

    public IReadOnlyList<string> PreReleaseIdentifiers
        => Array.AsReadOnly(GetComponents().PreReleaseIdentifiers);

    public IReadOnlyList<string> BuildMetadataIdentifiers
        => Array.AsReadOnly(GetComponents().BuildMetadataIdentifiers);

    public bool IsPreRelease => GetComponents().PreReleaseIdentifiers.Length > 0;

    public string? PreRelease
    {
        get
        {
            string[] identifiers = GetComponents().PreReleaseIdentifiers;
            return identifiers.Length == 0 ? null : string.Join('.', identifiers);
        }
    }

    public string? BuildMetadata
    {
        get
        {
            string[] identifiers = GetComponents().BuildMetadataIdentifiers;
            return identifiers.Length == 0 ? null : string.Join('.', identifiers);
        }
    }

    public static void Validate(string value)
    {
        if (!TryGetComponents(value, out _))
        {
            throw new DomainValidationException(
                $"'{value}' is not a valid Semantic Versioning 2.0.0 version.");
        }
    }

    public int CompareTo(SemanticVersion other)
    {
        return Compare(GetComponents(), other.GetComponents());
    }

    public static bool operator <(SemanticVersion left, SemanticVersion right)
        => left.CompareTo(right) < 0;

    public static bool operator <=(SemanticVersion left, SemanticVersion right)
        => left.CompareTo(right) <= 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right)
        => left.CompareTo(right) > 0;

    public static bool operator >=(SemanticVersion left, SemanticVersion right)
        => left.CompareTo(right) >= 0;

    private SemanticVersionComponents GetComponents()
    {
        string value = (string)this;
        if (!TryGetComponents(value, out SemanticVersionComponents components))
        {
            throw new InvalidOperationException(
                $"The initialized semantic version '{value}' is invalid.");
        }

        return components;
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

        int buildSeparator = value.IndexOf('+', StringComparison.Ordinal);
        string versionWithoutBuild = buildSeparator < 0 ? value : value[..buildSeparator];
        string? buildValue = buildSeparator < 0 ? null : value[(buildSeparator + 1)..];
        if (buildSeparator >= 0
            && (value.IndexOf('+', buildSeparator + 1) >= 0 || string.IsNullOrEmpty(buildValue)))
        {
            return false;
        }

        int preReleaseSeparator = versionWithoutBuild.IndexOf('-', StringComparison.Ordinal);
        string coreValue = preReleaseSeparator < 0
            ? versionWithoutBuild
            : versionWithoutBuild[..preReleaseSeparator];
        string? preReleaseValue = preReleaseSeparator < 0
            ? null
            : versionWithoutBuild[(preReleaseSeparator + 1)..];
        if (preReleaseSeparator >= 0 && string.IsNullOrEmpty(preReleaseValue))
        {
            return false;
        }

        string[] coreIdentifiers = coreValue.Split('.', StringSplitOptions.None);
        if (coreIdentifiers.Length != 3
            || !TryParseCoreIdentifier(coreIdentifiers[0], out BigInteger major)
            || !TryParseCoreIdentifier(coreIdentifiers[1], out BigInteger minor)
            || !TryParseCoreIdentifier(coreIdentifiers[2], out BigInteger patch))
        {
            return false;
        }

        string[] preReleaseIdentifiers = SplitIdentifiers(preReleaseValue);
        string[] buildMetadataIdentifiers = SplitIdentifiers(buildValue);
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
        int comparison = left.Major.CompareTo(right.Major);
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

        for (int index = 0;
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
    {
        return value is null ? [] : value.Split('.', StringSplitOptions.None);
    }

    private static bool IsValidIdentifier(string value)
    {
        return value.Length > 0
                && value.All(static character =>
                    char.IsAsciiLetterOrDigit(character) || character == '-');
    }

    private static bool IsNumericIdentifier(string value)
    {
        return value.All(char.IsAsciiDigit);
    }

    private static bool HasLeadingZero(string value)
    {
        return value.Length > 1 && value[0] == '0';
    }

    private static int ComparePreReleaseIdentifier(string left, string right)
    {
        bool leftNumeric = IsNumericIdentifier(left);
        bool rightNumeric = IsNumericIdentifier(right);
        if (leftNumeric != rightNumeric)
        {
            return leftNumeric ? -1 : 1;
        }

        if (!leftNumeric)
        {
            return string.Compare(left, right, StringComparison.Ordinal);
        }

        int comparison = left.Length.CompareTo(right.Length);
        return comparison == 0
            ? string.Compare(left, right, StringComparison.Ordinal)
            : comparison;
    }

    private readonly record struct SemanticVersionComponents(
        BigInteger Major,
        BigInteger Minor,
        BigInteger Patch,
        string[] PreReleaseIdentifiers,
        string[] BuildMetadataIdentifiers);
}
