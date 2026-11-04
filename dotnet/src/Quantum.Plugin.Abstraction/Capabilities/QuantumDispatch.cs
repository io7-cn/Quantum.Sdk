using System.Text.Json.Serialization;

namespace Quantum.Plugin.Abstraction;

[JsonConverter(typeof(JsonStringEnumConverter<QuantumDispatch>))]
public enum QuantumDispatch
{
    [JsonStringEnumMemberName("all")]
    All,
    [JsonStringEnumMemberName("ordered")]
    Ordered
}
