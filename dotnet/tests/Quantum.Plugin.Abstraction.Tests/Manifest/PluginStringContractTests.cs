using NOF.Domain;
using System.Numerics;
using System.Text.Json;
using Xunit;

namespace Quantum.Plugin.Abstraction.Tests;

public sealed class PluginStringContractTests
{
    [Fact]
    public void PluginId_NormalizesAndValidatesValues()
    {
        Assert.Equal("quantum.plugin.example", (string)PluginId.Of(" Quantum.Plugin.Example "));
        Assert.Equal(
            PluginId.MaximumLength,
            ((string)PluginId.Of($"a{new string('b', PluginId.MaximumLength - 1)}")).Length);
        Assert.Throws<DomainValidationException>(() => PluginId.Of("disabled"));
        Assert.Throws<DomainValidationException>(() => PluginId.Of("plugin/invalid"));
        Assert.Throws<DomainValidationException>(() => PluginId.Of($"a{new string('b', PluginId.MaximumLength)}"));
    }

    [Fact]
    public void SemanticVersion_ParsesComponentsAndComparesPrecedence()
    {
        var version = "1.2.3-rc.10+linux.arm64";
        var components = SemanticVersion.Components(version);

        Assert.Equal(new BigInteger(1), components.Major);
        Assert.Equal(new BigInteger(2), components.Minor);
        Assert.Equal(new BigInteger(3), components.Patch);
        Assert.Equal(["rc", "10"], components.PreReleaseIdentifiers);
        Assert.Equal(["linux", "arm64"], components.BuildMetadataIdentifiers);
        Assert.True(SemanticVersion.Compare(version, "1.2.3") < 0);
        Assert.True(SemanticVersion.Compare(version, "1.2.3-rc.2") > 0);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1.2")]
    [InlineData("01.2.3")]
    [InlineData("1.2.3-01")]
    [InlineData("1.2.3+")]
    public void SemanticVersion_RejectsNonSemVer20Values(string value)
        => Assert.Throws<DomainValidationException>(() => SemanticVersion.Of(value));

    [Fact]
    public void QuantumPluginInfo_PreservesStringJsonWireFormat()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var info = new QuantumPluginInfo("Quantum.Plugin.Example", "1.2.3-rc.1+build.7");

        var json = JsonSerializer.Serialize(info, options);
        var roundTrip = JsonSerializer.Deserialize<QuantumPluginInfo>(json, options);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("quantum.plugin.example", document.RootElement.GetProperty("id").GetString());
        Assert.Equal("1.2.3-rc.1+build.7", document.RootElement.GetProperty("version").GetString());
        Assert.NotNull(roundTrip);
        Assert.Equal(info.Id, roundTrip.Id);
        Assert.Equal(info.Version.ToString(), roundTrip.Version.ToString());
    }

    [Fact]
    public void QuantumPluginInfo_RejectsInvalidStringJsonValues()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        Assert.Throws<DomainValidationException>(() => JsonSerializer.Deserialize<QuantumPluginInfo>(
            """{"id":"plugin/invalid","version":"1.2.3"}""",
            options));
        Assert.Throws<DomainValidationException>(() => JsonSerializer.Deserialize<QuantumPluginInfo>(
            """{"id":"quantum.plugin.example","version":"1.2"}""",
            options));
    }
}
