using System.Text;
using System.Text.Json;

namespace Quantum.Plugin.Abstraction;

/// <summary>Validation shared by extension adapters. Business payload validation belongs to consumers.</summary>
public static class QuantumExtensionContract
{
    public const int MaximumPayloadBytes = 131072;

    public static string LocalId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length is < 1 or > 128 || value.Any(character =>
                character is not (>= 'a' and <= 'z' or >= '0' and <= '9' or '-' or '_' or '.')))
        {
            throw new ArgumentException("Extension IDs require 1–128 lowercase ASCII letters, digits, dots, hyphens or underscores.", nameof(value));
        }

        return value;
    }

    public static string PointId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var parts = value.Split('/');
        if (parts.Length != 2 || PluginId.Of(parts[0]) != parts[0])
        {
            throw new ArgumentException("Extension points require a canonical plugin-id/local-id.", nameof(value));
        }

        LocalId(parts[1]);
        return value;
    }

    public static int Version(int value)
        => value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));

    public static JsonElement Payload(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Undefined
            || Encoding.UTF8.GetByteCount(value.GetRawText()) > MaximumPayloadBytes)
        {
            throw new ArgumentException("Extension payloads must be JSON values of at most 128 KiB.", nameof(value));
        }

        // Match the depth accepted by the JSON iframe transport in both runtimes.
        using var document = JsonDocument.Parse(value.GetRawText(), new JsonDocumentOptions { MaxDepth = 64 });
        return document.RootElement.Clone();
    }
}
