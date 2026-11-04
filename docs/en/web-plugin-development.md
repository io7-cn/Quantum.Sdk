# Web plugin development

[简体中文](../zh-CN/web-plugin-development.md)

Use `@io7/quantum-plugin-sdk` with a compatible Quantum Desktop host. Web plugins run in a sandboxed [iframe](glossary.md#iframe). Initialize in `activate`, return a cleanup function and render in `mount`. The host remounts views when the language changes. Bundle the plugin into one ES module. See the [Web example](../../samples/Quantum.ExampleWebPlugin).

Use `context.request<T>()` with a capability name such as `example.suggestions`. Inspect the provider outcomes and `stopped` flag. Discover request and notification capabilities with `context.capabilities.catalog()`. The .NET plugin implements the request provider. Declare required and optional capabilities under `capabilities.requires` and `capabilities.integrates`.

Publish notifications with `context.notify.publish()` and subscribe with `context.notify.subscribe()`. Validate the JSON payload before use. Only observers declared in `stopObservers` may stop ordered propagation. Dispose subscriptions during cleanup.

Use the package's installation helpers with the plugin context. Save the operation ID returned after user consent and let the handler return. The same plugin may query that ID after reload. See the [.NET guide](dotnet-plugin-development.md) for shared semantics and [Mobile UI](mobile-ui.md) for declaring mobile pages.
