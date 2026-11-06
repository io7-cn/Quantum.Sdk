# Runtime extensions

[简体中文](../zh-CN/extensions.md)

The `0.3.0-rc.3` candidate adds code-defined extension points. No manifest contribution fields are required. An extension point is a canonical `plugin-id/local-id` string and a positive 32-bit contract version, not a CLR or TypeScript business type. Contributions may contain any JSON value, including null, arrays and scalars. Consumers validate their own business contracts and ignore or report invalid entries individually.

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

See the `Quantum.Desktop/docs/en/extensions.md` settings contract for one consumer, and [package verification](packages.md) for the candidate package workflow. The .NET sample defines a custom point and contributes a settings page; its component observes the point without sharing business types.

Each runtime is limited to 256 definitions/contributions and 128 observations. Slow observations coalesce queued snapshots after 32 pending updates; do not use this API to deliver every event.
