# Web 插件开发

[English](../en/web-plugin-development.md)

使用与 Quantum Desktop 兼容的 `@io7/quantum-plugin-sdk`。Web 插件在沙箱 [iframe](glossary.md#iframe) 中运行。在 `activate` 中初始化并返回清理函数，在 `mount` 中渲染。切换语言后宿主重新挂载视图。将插件打成一个 ES 模块，参见 [Web 示例](../../samples/Quantum.ExampleWebPlugin)。

用 `context.request<T>()` 按能力名发起请求，例如 `example.suggestions`。检查各提供方的结果和 `stopped` 标记，用 `context.capabilities.catalog()` 发现请求和通知能力。请求提供方由 .NET 插件实现。在 `capabilities.requires` 和 `capabilities.integrates` 中声明必需和可选能力。

用 `context.notify.publish()` 发布通知，用 `context.notify.subscribe()` 订阅。使用前校验 JSON 载荷。只有在 `stopObservers` 中声明的观察者可以停止有序传播。在清理函数中释放订阅。

把插件上下文传给安装辅助函数。用户同意后保存操作 ID，让当前处理器返回。同一插件可以在重载后查询该 ID。共享语义参见 [.NET 指南](dotnet-plugin-development.md)，移动页面声明参见[移动界面](mobile-ui.md)。
