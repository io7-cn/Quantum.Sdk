# .NET plugin development

[简体中文](../zh-CN/dotnet-plugin-development.md)

Target `net10.0` and reference a `Quantum.Plugin.Abstraction` version supported by your Quantum Desktop host. See the [example plugin](../../samples/Quantum.ExamplePlugin). Quantum finds `IQuantumPlugin` implementations in the entry assembly. Register services in `ConfigureServices`, initialize in `StartAsync`, and stop work and dispose subscriptions in `StopAsync`.

## Request capabilities

Expose [remote procedure calls](glossary.md#rpc) through NOF Contract. Annotate the service interface with `[TransportOverQuantum]` and each exposed method with `[RequestCapability]`. Reference `NOF.Application` to register the generated `RpcServer<T>` with `services.AddRpcServer<T>()`. List every provided capability in `capabilities.provides` in the manifest. Use stable lowercase names such as `notes.find`.

```csharp
var result = await requester.RequestAsync<Note[]>(
    "notes.find",
    new { text = "quantum" },
    Context.Empty,
    cancellationToken);
```

Check `result.IsSuccess` before reading `Outcomes` and `Stopped` from `result.Value`. Provider outcomes are `Handled`, `Pass`, `Failed` or `TimedOut`. `RequestSingleAsync<T>()` projects at most one non-Pass result. `All` dispatches concurrently. `Ordered` dispatches by order and plugin ID. Use `AnswerAndStop` only with ordered dispatch and `AllowStop`. A `UniqueOwner` request stops at the first non-Pass outcome. `RequireSingleProvider` rejects ambiguous writes with multiple candidates.

Call `DiscoverCapabilitiesAsync()` for the schema 2 capability catalog. It lists active request and notification capabilities, providers, schemas and dispatch policy. The package includes a Roslyn analyzer that checks missing declarations, invalid policies, unsupported return types and unauthorized stopping via diagnostics QTM001 through QTM004.

## Notifications and manifests

Publish through `IQuantumNotifier.PublishAsync` and receive through `Subscribe` or `Observe`. Read the payload as JSON or use `TryDeserialize<T>()`. Declare notification capabilities and their message schemas in `plugin.json`. Only observers listed in `stopObservers` can stop ordered propagation. Dispose subscriptions when the plugin stops.

Plugin IDs, versions and version ranges are strings in the contracts. Optional validators include `PluginId.Of`, `SemanticVersion.Of` and `VersionRange.Of`. The host validates and normalizes manifest values. Include numbered SQLite scripts under `database.migrations` for host managed migration before startup.

## Installing plugins

Use `InstallMarketplacePluginAsync`, `InstallLocalPluginAsync` and `GetPluginInstallStatusAsync` with `IQuantumRequester`. The host requests user consent and returns `pending` with an operation ID. Save the ID and let the current handler return. The same plugin can query the result after reloading.

See [Mobile UI](mobile-ui.md) and the [SDK glossary](glossary.md).
