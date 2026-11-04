# Mobile UI

[简体中文](../zh-CN/mobile-ui.md)

A plugin declares a versioned page on Quantum Desktop. The mobile client renders the page and sends opaque action IDs to the desktop. The plugin and its handlers run on the desktop. Match the desktop and mobile versions to the page schema in use.

Use `IQuantumMobilePages` with `Mobile` in .NET. In a Web plugin, use the `Mobile` builder and publish through `context.mobile`.

```csharp
using Quantum.Plugin.Abstraction;
using Ui = Quantum.Plugin.Abstraction.Mobile;

services.GetRequiredService<IQuantumMobilePages>().Publish(
    Ui.Page("quick-note", "Quick note", Ui.VStack("root",
        Ui.TextField("content").Hint("Content").Limit(500),
        Ui.Button("save", "Save").OnClick(
            Ui.Request("notes.create")
              .Constant("notebookId", "inbox")
              .Input("text", "content")))));
```

```typescript
import { definePlugin, Mobile, validateMobilePage } from "@io7/quantum-plugin-sdk";

export default definePlugin({
  async activate(context) {
    const page = Mobile.page("quick-note", "Quick note", Mobile.vStack("root",
      Mobile.textField("content").hint("Content").limit(500),
      Mobile.button("save", "Save").onClick(
        Mobile.request("notes.create")
          .constant("notebookId", "inbox").input("text", "content"))));
    validateMobilePage(page);
    await context.mobile.publish(page);
    return () => context.mobile.remove(page.id);
  }
});
```

Publishing the same page ID replaces the page and invalidates its old actions. Remove the page when the plugin stops. The host also removes Web pages when it disposes their frame. Publishing is synchronous in .NET and asynchronous in Web plugins. Mobile users refresh the view explicitly.

Actions use a request capability name and need an active provider. The host binds the button to the selected provider while the page is open. Inputs bind control values to top-level request fields. Constant and input fields cannot overlap. Use JSON values for constants. Pages support up to 500 nodes, 16 levels and 128 KiB. Strings have a 2,000-character limit and action input has a 16 KiB limit, excluding attachments. A call that reached the handler may have an unknown result after a network interruption. Handlers that need stronger guarantees can deduplicate by request ID.

Use `Mobile.Attachments(id)` or `Mobile.attachments(id)` to declare a native file, image or voice picker. Pages with an attachment control use schema 2. Other pages use schema 1. Bind the picker to a `MobileAttachment[]` request property in .NET. Each entry carries `name`, `contentType` and `dataBase64`. The limits are four files, 8 MiB per file and 16 MiB total. Schema 2 requires a compatible desktop and mobile client.

The [schema 1 example](../../fixtures/mobile-page-v1.json) is a test fixture for both SDKs. See the [glossary](glossary.md) for terminology.
