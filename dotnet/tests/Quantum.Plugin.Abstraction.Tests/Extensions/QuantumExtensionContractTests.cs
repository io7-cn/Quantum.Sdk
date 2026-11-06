using System.Text.Json;
using Xunit;

namespace Quantum.Plugin.Abstraction.Tests;

public sealed class QuantumExtensionContractTests
{
    [Theory]
    [InlineData("example.plugin/actions")]
    [InlineData("quantum.settings/pages")]
    public void CanonicalPointIdentifiersAreAccepted(string pointId)
        => Assert.Equal(pointId, QuantumExtensionContract.PointId(pointId));

    [Theory]
    [InlineData("Example.Plugin/actions")]
    [InlineData("example.plugin/actions/nested")]
    [InlineData("example.plugin/")]
    [InlineData("example.plugin/white space")]
    public void NoncanonicalPointIdentifiersAreRejected(string pointId)
        => Assert.ThrowsAny<Exception>(() => QuantumExtensionContract.PointId(pointId));

    [Fact]
    public void PayloadsRequireBoundedJsonButNoSpecificBusinessType()
    {
        Assert.Throws<ArgumentException>(() => QuantumExtensionContract.Payload(default));
        Assert.Throws<ArgumentOutOfRangeException>(() => QuantumExtensionContract.Version(0));
        var large = JsonSerializer.SerializeToElement(new string('x', QuantumExtensionContract.MaximumPayloadBytes));
        Assert.Throws<ArgumentException>(() => QuantumExtensionContract.Payload(large));
        using var deep = JsonDocument.Parse(new string('[', 65) + "0" + new string(']', 65),
            new JsonDocumentOptions { MaxDepth = 128 });
        Assert.ThrowsAny<JsonException>(() => QuantumExtensionContract.Payload(deep.RootElement));
        foreach (var json in new[] { "null", "false", "42", "[]", "{}", "\"text\"" })
        {
            using var document = JsonDocument.Parse(json);
            Assert.Equal(json, QuantumExtensionContract.Payload(document.RootElement).GetRawText());
        }
    }
}
