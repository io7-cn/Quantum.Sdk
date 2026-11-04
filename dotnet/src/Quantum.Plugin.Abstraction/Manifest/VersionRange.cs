using NOF.Domain;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Identifies a union of Semantic Versioning 2.0.0 intervals and finite sets.
/// </summary>
public static class VersionRange
{
    public static string Normalize(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return string.Join(
            '|',
            normalized.Split('|', StringSplitOptions.None)
                .Select(static term => NormalizeTerm(term.Trim())));
    }

    public static void Validate(string value)
        => _ = ParseTerms(value);

    public static string Of(string? value)
    {
        var range = Normalize(value);
        Validate(range);
        return range;
    }

    public static bool Contains(string range, string version)
    {
        SemanticVersion.Validate(version);
        return ParseTerms(Normalize(range)).Any(term => term.Contains(version));
    }

    private static string NormalizeTerm(string term)
    {
        if (string.Equals(term, "*", StringComparison.Ordinal))
        {
            return "(,)";
        }

        if (term.Length < 2)
        {
            return term;
        }

        var first = term[0];
        var last = term[^1];
        var values = term[1..^1].Split(',', StringSplitOptions.None);
        if ((first is '[' or '(') && (last is ']' or ')') && values.Length == 2)
        {
            return $"{first}{values[0].Trim()},{values[1].Trim()}{last}";
        }

        if (first == '{' && last == '}')
        {
            return $"{{{string.Join(',', values.Select(static item => item.Trim()))}}}";
        }

        return term;
    }

    private static VersionRangeTerm[] ParseTerms(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw InvalidRange(value);
        }

        var terms = value.Split('|', StringSplitOptions.None);
        if (terms.Any(string.IsNullOrEmpty))
        {
            throw InvalidRange(value);
        }

        return terms.Select(term => ParseTerm(value, term)).ToArray();
    }

    private static VersionRangeTerm ParseTerm(string range, string term)
    {
        if (term.Length < 2)
        {
            throw InvalidRange(range);
        }

        if (term[0] == '{' && term[^1] == '}')
        {
            var values = term[1..^1].Split(',', StringSplitOptions.None);
            if (values.Length == 0 || values.Any(string.IsNullOrEmpty))
            {
                throw InvalidRange(range);
            }

            return VersionRangeTerm.FiniteSet(values.Select(SemanticVersion.Of).ToArray());
        }

        var includeLowerBound = term[0] == '[';
        var includeUpperBound = term[^1] == ']';
        if (term[0] is not ('[' or '(') || term[^1] is not (']' or ')'))
        {
            throw InvalidRange(range);
        }

        var bounds = term[1..^1].Split(',', StringSplitOptions.None);
        if (bounds.Length != 2)
        {
            throw InvalidRange(range);
        }

        if ((bounds[0].Length == 0 && includeLowerBound)
            || (bounds[1].Length == 0 && includeUpperBound))
        {
            throw InvalidRange(range);
        }

        string? lowerBound = bounds[0].Length == 0
            ? null
            : SemanticVersion.Of(bounds[0]);
        string? upperBound = bounds[1].Length == 0
            ? null
            : SemanticVersion.Of(bounds[1]);

        if (lowerBound is { } lower && upperBound is { } upper)
        {
            var comparison = SemanticVersion.Compare(lower, upper);
            if (comparison > 0
                || (comparison == 0 && (!includeLowerBound || !includeUpperBound)))
            {
                throw InvalidRange(range);
            }
        }

        return VersionRangeTerm.Interval(
            lowerBound,
            includeLowerBound,
            upperBound,
            includeUpperBound);
    }

    private static DomainValidationException InvalidRange(string value)
        => new($"'{value}' is not a valid version range.");

    private readonly record struct VersionRangeTerm(
        VersionRangeTermKind Kind,
        string? LowerBound,
        bool IncludeLowerBound,
        string? UpperBound,
        bool IncludeUpperBound,
        string[]? Versions)
    {
        public static VersionRangeTerm Interval(
            string? lowerBound,
            bool includeLowerBound,
            string? upperBound,
            bool includeUpperBound)
            => new(
                VersionRangeTermKind.Interval,
                lowerBound,
                includeLowerBound,
                upperBound,
                includeUpperBound,
                Versions: null);

        public static VersionRangeTerm FiniteSet(string[] versions)
            => new(
                VersionRangeTermKind.FiniteSet,
                LowerBound: null,
                IncludeLowerBound: false,
                UpperBound: null,
                IncludeUpperBound: false,
                versions);

        public bool Contains(string version)
        {
            if (Kind == VersionRangeTermKind.FiniteSet)
            {
                return Versions!.Any(candidate => SemanticVersion.Compare(candidate, version) == 0);
            }

            if (LowerBound is { } lowerBound)
            {
                var lowerComparison = SemanticVersion.Compare(version, lowerBound);
                if (lowerComparison < 0 || (lowerComparison == 0 && !IncludeLowerBound))
                {
                    return false;
                }
            }

            if (UpperBound is { } upperBound)
            {
                var upperComparison = SemanticVersion.Compare(version, upperBound);
                if (upperComparison > 0 || (upperComparison == 0 && !IncludeUpperBound))
                {
                    return false;
                }
            }

            return true;
        }
    }

    private enum VersionRangeTermKind
    {
        Interval,
        FiniteSet
    }
}
