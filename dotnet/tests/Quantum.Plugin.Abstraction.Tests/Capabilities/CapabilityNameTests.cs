using Xunit;

namespace Quantum.Plugin.Abstraction.Tests;

public sealed class CapabilityNameTests
{
    [Theory]
    [InlineData("devices.camera.status")]
    [InlineData("device-1.camera_status.changed-v2")]
    public void PluginCapabilityNamesAcceptDotSeparatedSegments(string name)
        => Assert.True(CapabilityName.IsPluginCapability(name));

    [Theory]
    [InlineData("devices/status")]
    [InlineData("devices..status")]
    [InlineData(" devices.status")]
    [InlineData("Devices.status")]
    [InlineData("devices.status\n")]
    [InlineData("status")]
    [InlineData("quantum.devices.status")]
    public void PluginCapabilityNamesRejectInvalidOrReservedNames(string name)
        => Assert.False(CapabilityName.IsPluginCapability(name));

    [Fact]
    public void PluginCapabilityNamesAreLimitedTo200Characters()
    {
        Assert.True(CapabilityName.IsPluginCapability("a." + new string('a', 198)));
        Assert.False(CapabilityName.IsPluginCapability("a." + new string('a', 199)));
    }
}
