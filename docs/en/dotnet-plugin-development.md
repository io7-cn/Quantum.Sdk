# .NET plugin development

[简体中文](../zh-CN/dotnet-plugin-development.md)

Target `net10.0` and reference `Quantum.Plugin.Abstraction` at a version supported by your host. See the [example plugin](../../samples/Quantum.ExamplePlugin) and [dependent plugin](../../samples/Quantum.ExampleDependentPlugin) for working projects.

## Lifecycle

Quantum finds implementations of `IQuantumPlugin` in the entry assembly. It runs generated NOF initializers before the static `ConfigureServices` hook, which can add or override registrations. The host invokes `StartAsync` and `StopAsync` with the same runtime scope. Services shared across remote calls use singleton lifetime because each call gets its own target scope. Stop tasks and dispose subscriptions in `StopAsync`.

`IQuantumPluginEnvironment` reports available plugins. `IQuantumPluginRuntimeContext` exposes the installed version and the read-only shadow directory for the current runtime.

## Identity and version

`PluginId.Of` trims whitespace, normalizes to lowercase and validates the permitted characters and the 128-character limit. The host reserves `disabled`. `SemanticVersion.Of` accepts all three numeric components of [SemVer](glossary.md#semver) 2.0.0. Precedence includes prerelease identifiers and ignores build metadata. `VersionRange.Of` accepts intervals, sets and unions. `(,)` and `*` both match all versions. Both SDKs serialize these values to JSON strings.

```csharp
var pluginId = PluginId.Of("quantum.plugin.example");
var version = SemanticVersion.Of("1.2.3+linux");
var supported = VersionRange.Of("{1.2.3} | [1.3.0,2.0.0)").Contains(version);
```

## Remote procedure calls

When hosting a NOF server, reference `NOF.Application` and register it using `services.AddRpcServer<TServer>()`. Use `TransportOverQuantum`, `RpcInvocationName` and `RpcInvocationAlias` to publish names. A consumer resolves `IRpcInvoker` from its own service container.

```csharp
var result = await invoker.InvokeAsync<Note[]>(
    "quantum.plugin.notes.notes.find",
    new { text = "quantum" },
    Context.Empty,
    cancellationToken);
```

Use canonical names for capabilities and fully qualified names to target a particular implementation. Names ignore case. The host prefers a declared active provider and uses ordinal plugin ID ordering between providers. An unknown name returns a failed result with `rpc_not_found`. Check `IsSuccess` before reading the value. `GetCatalogAsync` lists callable names, input and output schemas, and declaration metadata. The host serializes each call and disposes its target scope after producing the result.

The manifest uses `capabilities.provides`, `capabilities.requires` and `capabilities.integrates` for provided, required and optional capabilities. Check the catalog before offering an optional action.

## Installing plugins

Use `InstallMarketplacePluginAsync`, `InstallLocalPluginAsync` and `GetPluginInstallStatusAsync` on `IRpcInvoker`. For a local installation, supply a display filename and ZIP bytes. The host checks compatibility, dependencies and its 512 MiB ZIP limit.

The host shows the request to the user. Once accepted, it returns `pending` and an operation ID. Save the ID and let the current handler return so the host can reload plugins. The same plugin can query the ID after reloading for `succeeded` or `failed`. The host retains the most recent 64 operations in memory. Cancellation applies until the host accepts the request.

## Events and plugin data

Resolve `IQuantumEventBus` from the plugin services, create a publisher with a validated `QuantumTopic`, and retain subscriptions for disposal in `StopAsync`. Handlers receive a `QuantumEvent` containing JSON. Use `DeserializeRequired<T>()` when a matching payload is required. Topics have a 255-character limit and use dot-separated names such as `devices.status`. The host publishes events in memory without replay.

Ship numbered SQLite migrations under `database.migrations` in `plugin.json`. The host applies pending migrations before starting the plugin. Use backward-compatible migrations when shipping updates.

See [Mobile UI](mobile-ui.md) and the [SDK glossary](glossary.md).
