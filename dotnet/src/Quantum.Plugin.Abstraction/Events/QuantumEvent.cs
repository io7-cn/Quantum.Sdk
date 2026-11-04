using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Quantum.Plugin.Abstraction;

/// <summary>
/// Contains a JSON payload and host-assigned publication metadata.
/// </summary>
public sealed record QuantumEvent(
    Guid Id,
    QuantumTopic Topic,
    JsonElement Payload,
    QuantumPluginInfo Publisher,
    DateTimeOffset PublishedAt)
{
    /// <summary>
    /// Deserializes <see cref="Payload"/> using web JSON defaults unless options are supplied.
    /// </summary>
    public TMessage? Deserialize<TMessage>(JsonSerializerOptions? options = null)
    {
        return Payload.Deserialize<TMessage>(CreateSerializerOptions(options));
    }

    /// <summary>
    /// Deserializes <see cref="Payload"/> and rejects a JSON <see langword="null"/> result.
    /// </summary>
    /// <exception cref="JsonException">The payload is invalid or deserializes to null.</exception>
    public TMessage DeserializeRequired<TMessage>(JsonSerializerOptions? options = null)
    {
        return Deserialize<TMessage>(options)
                ?? throw new JsonException(
                    $"Topic '{Topic}' produced a null {typeof(TMessage).FullName} payload.");
    }

    /// <summary>
    /// Deserializes <see cref="Payload"/> to a CLR type selected at runtime.
    /// </summary>
    public object? Deserialize(Type messageType, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(messageType);
        return Payload.Deserialize(messageType, CreateSerializerOptions(options));
    }

    /// <summary>
    /// Attempts to deserialize <see cref="Payload"/> without propagating malformed JSON errors.
    /// </summary>
    public bool TryDeserialize<TMessage>(
        out TMessage? message,
        JsonSerializerOptions? options = null)
    {
        try
        {
            message = Deserialize<TMessage>(options);
            return true;
        }
        catch (JsonException)
        {
            message = default;
            return false;
        }
    }

    private static JsonSerializerOptions CreateSerializerOptions(JsonSerializerOptions? options)
    {
        return options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
    }
}
