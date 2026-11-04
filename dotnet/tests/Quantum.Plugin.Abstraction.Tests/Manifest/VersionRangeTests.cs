using NOF.Domain;
using System.Text.Json;
using Xunit;

namespace Quantum.Plugin.Abstraction.Tests;

public sealed class VersionRangeTests
{
    [Fact]
    public void Contains_AppliesInclusiveAndExclusiveIntervalBounds()
    {
        var range = VersionRange.Of("[1.0.0,1.2.0)");

        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.0.0")));
        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.1.9")));
        Assert.False(VersionRange.Contains(range, SemanticVersion.Of("0.9.9")));
        Assert.False(VersionRange.Contains(range, SemanticVersion.Of("1.2.0")));
    }

    [Fact]
    public void Contains_SupportsUnboundedIntervalsAndWildcardAlias()
    {
        var below = VersionRange.Of("(,1.2.0)");
        var above = VersionRange.Of("(1.3.0,)");
        var all = VersionRange.Of("*");

        Assert.True(VersionRange.Contains(below, SemanticVersion.Of("0.0.0")));
        Assert.False(VersionRange.Contains(below, SemanticVersion.Of("1.2.0")));
        Assert.False(VersionRange.Contains(above, SemanticVersion.Of("1.3.0")));
        Assert.True(VersionRange.Contains(above, SemanticVersion.Of("999999999999999999999.0.0")));
        Assert.True(VersionRange.Contains(all, SemanticVersion.Of("1.2.3-alpha+build.1")));
        Assert.Equal(VersionRange.Of("(,)"), all);
        Assert.Equal("(,)", all.ToString());
    }

    [Fact]
    public void Contains_SupportsFiniteSetsAndIgnoresBuildMetadata()
    {
        var range = VersionRange.Of("{1.2.3,1.2.4+source}");

        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.2.3+abc")));
        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.2.4+linux-x64")));
        Assert.False(VersionRange.Contains(range, SemanticVersion.Of("1.2.5")));
    }

    [Fact]
    public void Contains_SupportsUnionsAndPrereleasePrecedence()
    {
        var range = VersionRange.Of(
            " {1.2.3} | [1.3.0,1.4.0) | [1.4.0-alpha,1.5.0) ");

        Assert.Equal("{1.2.3}|[1.3.0,1.4.0)|[1.4.0-alpha,1.5.0)", range.ToString());
        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.2.3+build")));
        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.3.5")));
        Assert.True(VersionRange.Contains(range, SemanticVersion.Of("1.4.0-beta")));
        Assert.False(VersionRange.Contains(range, SemanticVersion.Of("1.2.4")));
        Assert.False(VersionRange.Contains(range, SemanticVersion.Of("1.5.0")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1.2.3")]
    [InlineData("{}")]
    [InlineData("[,1.2.0)")]
    [InlineData("(1.3.0,]")]
    [InlineData("[1.2.0,1.2.0)")]
    [InlineData("[2.0.0,1.0.0]")]
    [InlineData("{1.2.3,}")]
    [InlineData("[1.0.0,1.2.0)||{2.0.0}")]
    [InlineData("**")]
    public void Of_RejectsInvalidRanges(string value)
        => Assert.Throws<DomainValidationException>(() => VersionRange.Of(value));

    [Fact]
    public void JsonConverter_RoundTripsAsNormalizedString()
    {
        var range = VersionRange.Of(" {1.2.3} | (1.3.0,) ");

        var json = JsonSerializer.Serialize(range);
        var restored = JsonSerializer.Deserialize<string>(json);

        Assert.Equal("\"{1.2.3}|(1.3.0,)\"", json);
        Assert.Equal(range, restored);
    }
}
