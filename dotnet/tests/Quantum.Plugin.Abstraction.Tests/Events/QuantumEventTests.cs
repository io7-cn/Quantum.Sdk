using System.Text.Json;
using Xunit;

namespace Quantum.Plugin.Abstraction.Tests;

public sealed class QuantumEventTests
{
    [Fact]
    public void Deserialize_UsesWebDefaultsAndSupportsGenericAndRuntimeTypes()
    {
        QuantumEvent @event = CreateEvent("""{"state":"ready","sequence":7}""");

        PayloadMessage? generic = @event.Deserialize<PayloadMessage>();
        object? runtime = @event.Deserialize(typeof(PayloadMessage));

        Assert.Equal(new PayloadMessage("ready", 7), generic);
        Assert.Equal(new PayloadMessage("ready", 7), runtime);
    }

    [Fact]
    public void DeserializeRequired_RejectsJsonNull()
    {
        QuantumEvent @event = CreateEvent("null");

        Assert.Null(@event.Deserialize<PayloadMessage>());
        Assert.Throws<JsonException>(() =>
            @event.DeserializeRequired<PayloadMessage>());
    }

    [Fact]
    public void TryDeserialize_ReturnsFalseForAnIncompatiblePayload()
    {
        QuantumEvent @event = CreateEvent("""{"state":"ready","sequence":"invalid"}""");

        bool succeeded = @event.TryDeserialize<PayloadMessage>(out PayloadMessage? message);

        Assert.False(succeeded);
        Assert.Null(message);
    }

    private static QuantumEvent CreateEvent(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        return new QuantumEvent(
            Guid.NewGuid(),
            QuantumTopic.Of("devices.status"),
            document.RootElement.Clone(),
            new QuantumPluginInfo("quantum.plugin.publisher", "1.0.0"),
            DateTimeOffset.UtcNow);
    }

    private sealed record PayloadMessage(string State, int Sequence);
}
