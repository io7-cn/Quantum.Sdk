# Web 插件开发

[English](../en/web-plugin-development.md)

`@io7/quantum-plugin-sdk` 为实验性 Web 插件提供类型和宿主契约。选择与目标 Quantum Desktop 兼容的版本。插件在独立沙箱 [iframe](glossary.md#iframe) 内运行，卸载时宿主销毁该框架。

在 `activate` 中初始化状态并返回清理函数，在 `mount` 中渲染视图。通过 `context` 使用宿主服务。视图接收宿主选择的语言、区域和文字方向。切换语言后，宿主重新挂载视图。Web 插件没有 .NET 依赖注入容器和 `ConfigureServices` 钩子。

将依赖打入单个自包含 ES 模块，参见[可运行示例](../../samples/Quantum.ExampleWebPlugin)。数据库迁移随插件包提交，在 `plugin.json` 声明 `database.migrations`。宿主在激活插件前应用迁移。

```ts
import { definePlugin, QuantumTopic } from "@io7/quantum-plugin-sdk";

export default definePlugin({
  async activate(context) {
    await context.log.info("Plugin activated");
    const topic = QuantumTopic.of("devices.status");
    const subscription = await context.eventBus.subscribe(topic, event => {
      const payload = event.payload as { state?: string };
      return context.log.info(`${event.publisher.id}: ${payload.state ?? "unknown"}`);
    });
    const publisher = context.eventBus.createPublisher<{ state: string }>(topic);
    await publisher.publish({ state: "ready" });
    return () => subscription.dispose();
  },

  mount({ element, route, locale, signal }) {
    element.textContent = `${route.path} (${locale.cultureName})`;
    signal.addEventListener("abort", () => element.replaceChildren(), { once: true });
  }
});
```

## 远程调用

通过 `context.rpc.invoke<T>()` 按规范名称调用能力，指定某个实现时使用完整名称。短名称和别名忽略大小写，冲突时宿主按规范化插件 ID 的序号顺序选取实现。找不到名称时返回错误码为 `rpc_not_found` 的失败结果。检查 `isSuccess` 后再读取值。

```ts
const result = await context.rpc.invoke<Note[]>(
  "quantum.plugin.notes.notes.find",
  { text: "quantum" },
  {},
  { signal: context.signal });
if (result.isSuccess) {
  console.log(result.value);
}
```

在清单中用 `capabilities.requires` 声明必需能力，用 `capabilities.integrates` 声明可选能力。使用 `context.rpc.catalog()` 查询当前可用的名称、输入输出结构和声明元数据。Web 插件通过框架消息使用远程调用，由 .NET 插件提供 NOF 服务端实现。

## 标识、版本和事件

使用 `PluginId.of()` 验证标识，使用 `SemanticVersion.parse()` 和 `SemanticVersion.compare()` 处理语义化版本。标识最多 128 字符，保留值为 `disabled`。版本比较考虑预发布标识，忽略构建元数据。`VersionRange.parse()` 接受区间、集合与并集，`(,)` 和 `*` 匹配所有版本。上述类型在 JSON 中均为字符串。

使用 `QuantumTopic.of()` 创建点分主题，最长 255 字符。事件的 `payload` 是 JSON 值，处理前先校验或收窄类型。发布会等待订阅者，宿主会向发布者返回订阅处理失败。卸载时释放订阅，宿主也会在框架销毁时清理剩余订阅。事件保存在内存中，不会重放历史事件。

## 安装插件

将 `context.rpc` 传给 `installMarketplacePlugin`、`installLocalPlugin` 或 `getPluginInstallStatus`。本地安装传入展示文件名及标准 Base64 ZIP 内容。宿主验证依赖、版本和 512 MiB 的 ZIP 大小限制。

用户接受请求后，宿主返回 `pending` 和操作 ID。保存该 ID 并让当前处理器返回，宿主随后重载插件。同一插件可在重载后查询 `succeeded` 或 `failed`。宿主在进程中保留最近 64 条操作，取消信号在请求获接受前有效。

参见[移动界面](mobile-ui.md)和[术语表](glossary.md)。
