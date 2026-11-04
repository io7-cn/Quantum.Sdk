# 移动界面

[English](../en/mobile-ui.md)

插件在 Quantum Desktop 声明版本化页面，移动端负责渲染。请按页面使用的版本选择兼容的 [SDK](glossary.md#sdk)、桌面端和移动端。

手机只接收版本化控件树和不透明动作 ID。界面声明、插件代码和 [RPC](glossary.md#rpc) 参数组装均在桌面。已支持 VStack、HStack、Scroll、Text、TextField、Toggle、Button、Divider、Spacer 和 Progress。

使用 `Quantum.Plugin.Abstraction.Mobile` 构造页面。页面 DTO 使用必填成员与属性初始化；ActionId、InputIds、设备注册和通信客户端属于框架内部，不是插件 API。

## .NET

在插件 StartAsync 从 [DI](glossary.md#di) 取得 `IQuantumMobilePages`，发布页面：

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

页面包含当下的展示数据，不包含移动执行的委托。再次 Publish 同一 pageId 替换页面并使旧动作失效；StopAsync 调用 Remove。插件也可在本地业务状态改变时重新发布。用户在手机点击刷新获得新版本，当前没有桌面数据变更自动刷新正在编辑的页面。

## TypeScript

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

TS 的 publish/remove 是实际 [iframe](glossary.md#iframe)→Host capability，不是仅类型声明。Host 验证当前插件与 runtime 所有权，并在 iframe runtime 释放时清理页面。.NET 发布是进程内同步操作；TS 发布因跨 iframe 消息边界为 Promise，这是传输方式导致的明确差异。两者经过同一 Host 校验器，错误分别为异常与 rejected Promise；不发起业务 RPC，所以没有在 Publish 中取消已执行业务的语义。

## 动作规则

- 按钮按请求能力名调用，例如 `notes.create`。宿主打开页面时绑定已启动的提供方，提供方替换后需刷新页面。
- 常量与输入字段不能重叠；Input 的 source 必须是页面内 TextField/Toggle ID。首版不支持嵌套路径、模板表达式、脚本或动态方法名。
- .NET Constant 接受 JSON 基本值或 JsonElement；结构化常量先由插件自己的序列化器转成 JsonElement，避免协议层持有插件私有 CLR 类型。TS 常量直接使用等价 JSON 数据。
- 控件 ID、页面 ID 和参数字段使用 1–128 个字母、数字、点、下划线、连字符，首字符是字母或数字。
- 单页上限 128 KiB、500 节点、16 层；字符串最长 2,000 字符，动作输入最多 16 KiB。
- 业务仍由现有 NOF Handler 校验。客户端验证、页面共享和设备授权不代替业务规则。
- `quantum.mobile.*` 为 Host 保留 Context 前缀；设备、requestId、viewId 由网关注入，.NET/TS 普通 RPC 调用者不能伪造。
- 同一动作请求 ID 的网络重投不会再次执行；进入 RPC 后结果未知时不会自动重做。业务需要更强保证时必须自行按 requestId 实现幂等。

## 附件（结构版本 2）

用 `Mobile.Attachments(id)` 或 `Mobile.attachments(id)` 声明原生文件、图片或语音选择控件。包含附件控件的页面使用结构版本 2，其余页面使用版本 1。附件通过常规输入映射绑定到请求的 `MobileAttachment[]` 属性，传输字段为 `name`、`contentType` 和 `dataBase64`。最多四个文件，单个文件最大 8 MiB，总计最大 16 MiB。使用结构版本 2 时需要兼容的桌面端和移动端。

[版本 1 示例](../../fixtures/mobile-page-v1.json)是两套 SDK 共用的测试数据。参见[术语表](glossary.md)。
