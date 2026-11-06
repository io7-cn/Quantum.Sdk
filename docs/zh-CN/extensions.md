# 运行时扩展

[English](../en/extensions.md)

`0.3.0-rc.3` 候选版本增加了通过代码定义的扩展点，无需新增清单字段。扩展点由规范的 `插件ID/本地ID` 字符串和正的 32 位契约版本组成，不绑定 CLR 或 TypeScript 业务类型。贡献可以是任意 JSON 值，包括 null、数组和标量。消费者负责校验自己的业务契约，并逐项忽略或报告无效贡献。

## .NET

从插件运行时服务容器解析 `IQuantumExtensions`。`Define(localId, contractVersion)` 定义属于当前插件的扩展点；`Register(pointId, contractVersion, localId, value, order)` 贡献脱离原文档生命周期的 `JsonElement`；`Observe(pointId, contractVersion, observer)` 返回可释放订阅，先交付初始快照，再串行交付版本递增的快照，不存在先读后订阅的空隙。回调在发布线程执行，更新组件前应切换到界面线程。观察者异常只记日志，不影响发布。

```csharp
var extensions = services.GetRequiredService<IQuantumExtensions>();
var definition = extensions.Define("actions", 1);
using var document = JsonDocument.Parse("""{"label":"Export"}""");
var contribution = extensions.Register("example.plugin/actions", 1, "export", document.RootElement);
var observation = extensions.Observe("example.plugin/actions", 1, snapshot =>
{
    // 检查 IsAvailable，并在使用 Value 前校验各项贡献。
});
```

示例假定当前插件 ID 为 `example.plugin`。调用 `contribution.Update(value)` 替换数据，释放句柄撤销贡献。句柄应由普通插件服务或组件状态管理。重复释放无副作用；已执行的回调可能在释放订阅后完成，但排队回调不再开始。插件停止后不要保留回调。使用局部序列化选项或 `JsonDocument` 构造 JSON，缓存插件自定义类型的全局序列化器可能阻止卸载。

## TypeScript

`context.extensions` 提供参数相同的异步 `define`、`register` 和 `observe`。所有句柄提供 `dispose()`，贡献句柄另有 `update(value)`。iframe 桥接传输数据与快照通知，不传递函数。观察回调应同步快速返回，异步工作自行处理拒绝。

```typescript
const point = await context.extensions.define("actions", 1);
const entry = await context.extensions.register(
  `${context.plugin.id}/actions`, 1, "export", { label: "Export" });
const watch = await context.extensions.observe(
  `${context.plugin.id}/actions`, 1, snapshot => console.log(snapshot));
// 在插件清理时：
await watch.dispose();
await entry.dispose();
await point.dispose();
```

.NET 注册同步生效，Web 方法异步确认跨帧操作。Web 通知丢弃过期或乱序的修订号。修订号只用于宿主会话内排序，不应持久保存；快照表示当前状态，不是事件日志。

## 身份、生命周期与限制

宿主提供贡献者身份和运行时代次。定义只能位于调用插件的命名空间；贡献本地 ID 在该运行时的所有贡献中唯一，扩展点本地 ID 在所有定义中唯一。本地 ID 使用 1–128 个小写 ASCII 字母、数字、点、连字符或下划线。契约版本精确匹配，不匹配的贡献保持待绑定。JSON 载荷最多 128 KiB UTF-8、64 层容器嵌套，不接受 undefined 和非 JSON 值。知道扩展点 ID 的插件都可观察它，因此禁止贡献秘密或可执行对象。

贡献可以先于定义存在。只有贡献方和定义方均在已发布目录中，且存在匹配定义时，贡献才可见。启动期间的贡献在目录提交后发布。停止或释放会清理注册与订阅，重新启动需要重新注册。旧句柄不能改变替代注册。可选扩展关系不建立启动依赖；只有无法独立工作的插件才声明真正的依赖。插件自定义扩展点与宿主扩展点遵循相同规则。

重复 ID 抛出 `InvalidOperationException`；非法参数抛出参数异常；失效句柄和作用域通过 `ObjectDisposedException` 拒绝更新。Web 对应操作拒绝 Promise。观察者异常被隔离。操作只替换数据或移除句柄，不自动重试用户动作。注册和通知只在本地会话中存在，不是持久存储，也不是可信 .NET 插件的安全隔离边界。

参见作为消费者示例的 `Quantum.Desktop/docs/zh-CN/extensions.md` 设置契约 和[包验证流程](packages.md)。.NET 示例定义自有扩展点、贡献设置页，并在组件中观察扩展点，无需共享业务类型。

每个运行时最多注册 256 个定义或贡献、128 个观察订阅。慢观察者积压达到 32 个快照后合并待处理状态，因此不能用该接口传递必须逐条交付的事件。
