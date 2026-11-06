# Runtime extensions

[简体中文](../zh-CN/extensions.md)

Plugins define extension points through code. No manifest contribution fields are required. An extension point is a canonical `plugin-id/local-id` string and a positive 32-bit contract version, not a CLR or TypeScript business type. Contributions may contain any JSON value, including null, arrays and scalars. Consumers validate their own business contracts and ignore or report invalid entries individually.

## .NET

Resolve `IQuantumExtensions` from the plugin runtime service provider. `Define(localId, contractVersion)` defines a point owned by the current plugin. `Register(pointId, contractVersion, localId, value, order)` contributes detached `JsonElement` data. `Observe(pointId, contractVersion, observer)` returns a disposable observation and delivers an initial snapshot followed by serialized, increasing-revision snapshots without a read/subscribe gap. Callbacks run on the publishing thread; dispatch to the UI thread before changing components. Observer failures are logged and do not reject publication.

```csharp
var extensions = services.GetRequiredService<IQuantumExtensions>();
var definition = extensions.Define("actions", 1);
using var document = JsonDocument.Parse("""{"label":"Export"}""");
var contribution = extensions.Register("example.plugin/actions", 1, "export", document.RootElement);
var observation = extensions.Observe("example.plugin/actions", 1, snapshot =>
{
    // Check IsAvailable and validate each contribution before using Value.
});
```

The example assumes the caller's plugin ID is `example.plugin`. `contribution.Update(value)` replaces its payload; dispose a handle to remove it. Keep handles in ordinary plugin services or component state. Disposal is idempotent. Callbacks already executing may finish after observation disposal; no queued callback starts afterwards. Do not retain callbacks after plugin stop. Build JSON with scoped serializer options or `JsonDocument`; global serializer caches containing plugin-defined types can prevent unload.

## TypeScript

`context.extensions` provides asynchronous `define`, `register` and `observe` methods with the same arguments. Returned handles provide `dispose()`; contribution handles also provide `update(value)`. The iframe bridge transfers data and snapshot notifications, never functions. Callbacks are synchronous and should return promptly; asynchronous work must handle its own rejection.

```typescript
const point = await context.extensions.define("actions", 1);
const entry = await context.extensions.register(
  `${context.plugin.id}/actions`, 1, "export", { label: "Export" });
const watch = await context.extensions.observe(
  `${context.plugin.id}/actions`, 1, snapshot => console.log(snapshot));
// In plugin cleanup:
await watch.dispose();
await entry.dispose();
await point.dispose();
```

The host applies registrations synchronously in .NET; Web methods acknowledge cross-frame operations asynchronously. Web notifications discard stale or out-of-order revisions. Revisions are ordering tokens within a host session, not persistent versions. A snapshot is the current state, not an event log.

## Identity, lifecycle and limits

The host supplies contributor identity and runtime generation. Definitions are scoped to the caller's namespace; a contribution local ID is unique across that caller's registrations, and a point local ID is unique across its definitions. Local IDs use 1–128 lowercase ASCII letters, digits, dots, hyphens or underscores. Point owners select exact contract versions; mismatched contributions remain pending. Payloads are JSON, at most 128 KiB UTF-8 and 64 nested containers. Undefined and non-JSON values are rejected. Never put secrets or executable objects in a contribution: any plugin knowing the point ID can observe it.

Contributions can precede definitions. They become visible only while both runtimes are in the committed catalog and a matching definition exists. Startup contributions remain staged until catalog publication. Stopping/disposal releases registrations and observations; restart requires fresh registration. An old handle cannot change a replacement registration. Optional extensions do not establish startup dependencies. Declare a real dependency only when the plugin cannot operate without its provider. Plugin-provided definitions and contributions follow identical rules to host extensions.

Duplicate registration IDs raise `InvalidOperationException`; malformed input raises argument exceptions; expired handles/scopes reject updates with `ObjectDisposedException`. Web methods reject their promise for the equivalent failures. Observer exceptions are isolated. Operations replace data or remove a handle and do not retry user actions automatically. Registration and delivery are in-process, session-only operations; they are not durable storage or an authorization boundary for trusted .NET plugins.

See `Quantum.Desktop/docs/en/extensions.md` for the host settings contract and [package verification](packages.md) for build and package checks.

Each runtime is limited to 256 definitions/contributions and 128 observations. Slow observations coalesce queued snapshots after 32 pending updates; do not use this API to deliver every event.

## Try the example plugins

Build and install the [main .NET example](../../samples/Quantum.ExamplePlugin/ExamplePlugin.cs), [companion .NET example](../../samples/Quantum.ExampleDependentPlugin/DependentPlugin.cs) and [Web example](../../samples/Quantum.ExampleWebPlugin/src/extension-example.ts) using the development guides. No settings or extension declarations are added to their manifests.

| Plugin | What it demonstrates |
| --- | --- |
| Main .NET example | Defines `quantum.plugin.example/example-data`, contributes its own message and adds an Example plugin settings tab. |
| Companion .NET example | Contributes to the main plugin's point without sharing a business type, and adds a section to General settings. |
| Web example | Defines its own point, contributes a message to the .NET example's point and adds an interactive Web settings tab. |

Use Open plugin settings on the example’s main page, or open Settings → Example plugin. Its message list identifies contributors and combines the main, companion and Web messages. Use Update message, Withdraw my message and Restore my message to exercise replacement, disposal and registration. Other plugins' entries remain visible when the main example withdraws only its own message.

Open Settings → Web example and update its message. Both that tab and the main .NET example's tab show the new value. Disable the companion plugin in Plugin management: its message and General section disappear; re-enable it and they return. The Web contribution can remain pending while the main point is unavailable and bind again when it returns.

The example payload contract is a JSON object with a string `message` and optional string `messageZh`; consumers ignore malformed entries. [ExampleSettings.razor](../../samples/Quantum.ExamplePlugin/ExampleSettings.razor) shows observation and validation. [ExampleExtensions.cs](../../samples/Quantum.ExamplePlugin/ExampleExtensions.cs) owns the main registrations in an ordinary service and releases them on stop. The Web view releases its observation on unmount, and plugin cleanup releases its registrations.
