# .NET 插件开发

[English](../en/dotnet-plugin-development.md)

插件以 `net10.0` 为目标，并引用与 Quantum Desktop 版本兼容的 `Quantum.Plugin.Abstraction`。参见[插件示例](../../samples/Quantum.ExamplePlugin)。宿主从入口程序集发现 `IQuantumPlugin` 实现。插件在 `ConfigureServices` 注册服务，在 `StartAsync` 启动，在 `StopAsync` 停止任务并释放订阅。

## 声明和调用能力

服务端使用 NOF Contract，并为每个 RPC 方法声明能力：

```csharp
[TransportOverQuantum]
public interface INotesService : IRpcService
{
    [RequestCapability("notes.find", QuantumDispatch.All)]
    Result<Note[]> Find(FindNotesRequest request);
}

public partial class NotesService : RpcServer<INotesService>;

public sealed class FindNotes(NoteStore store) : NotesService.Find
{
    public override Task<Result<Note[]>> HandleAsync(
        FindNotesRequest request,
        Context context,
        CancellationToken cancellationToken)
        => store.FindAsync(request, cancellationToken);
}
```

每个 RPC 方法通过 [RequestCapability] 声明一个稳定的能力名。这里的调用名是 notes.find；它不随插件 ID、CLR 接口名或方法名变化。名称区分大小写，采用小写 ASCII 字母、数字、连字符或下划线组成的点分段，最多 200 字符。quantum. 保留给宿主。

调用方从自己的 DI 获取 `IQuantumRequester`：

```csharp
var result = await services.GetRequiredService<IQuantumRequester>().RequestAsync<Note[]>(
    "notes.find",
    new { text = "quantum" },
    Context.Empty,
    cancellationToken);
```

没有可用提供方时，`outcomes` 为空。调用方按每个 outcome 的状态处理结果。Payload、Context 与 Result
始终经过 JSON 边界；每次调用在目标插件中创建独立 scope，结果序列化完成后释放。插件在 manifest 的 `capabilities.provides`、`capabilities.requires` 和
`capabilities.integrates` 中分别声明提供、强依赖和可选联动的能力名。宿主加载时要求 `provides` 与程序集提供的能力集合完全一致。
每项强依赖都由至少一个已启动的声明提供方满足。调用方使用 `IQuantumRequester` 传递
能力名，例如 `calendar.get`，Host 按能力声明的策略分发。
可选联动可通过 `quantum.capabilities.list` 检查。多个提供方实现同一能力时，需要遵循同一份请求和响应契约。

需要向 AI 工具或动态调用器导出当前能力时，可读取 Host 的内置 RPC 目录：

```csharp
var result = await services.GetRequiredService<IQuantumRequester>()
    .DiscoverCapabilitiesAsync(cancellationToken);
```

目录使用 schema 2：`capabilities` 为平铺列表。每项包含 `name`、所有当前提供方 `providerPluginIds`、输入和输出 JSON Schema，以及方法、参数和返回值的 Attribute 元数据。DTO 类型和属性上的描述保留在 Schema 中。

### 从旧版 SDK 迁移

用方法上的 [RequestCapability] 替换旧的命名 Attribute，把完整能力列表写入 capabilities.provides。所有调用和移动按钮改用能力名；目录消费者读取 schema 2 的 capabilities。旧 .NET 插件需要重新构建，旧 Web 插件也应更新调用与目录读取代码。

## Request 与 Notify

用 `IQuantumRequester.RequestAsync<T>()` 或 `context.request<T>()` 调用请求能力。统一目录 `quantum.capabilities.list` 列出 `request` 和 `notify` 能力，以及当前提供方、JSON Schema 和分发策略。.NET 使用 `DiscoverCapabilitiesAsync()`，Web 使用 `context.capabilities.catalog()`。

| 分发策略 | Request | Notify |
| --- | --- | --- |
| `All` | 并发请求所有匹配提供方 | 向所有匹配的观察者投递 |
| `Ordered` | 按 `Order`、插件 ID 的顺序请求。能力声明 `AllowStop` 时，提供方可随答复请求停止 | 按观察者的 `order`、插件 ID 的顺序投递。只有声明在 `stopObservers` 中的插件能停止后续传播 |

Request 返回 `outcomes` 和 `stopped`。每项结果带提供方插件 ID，状态为 `handled`、`pass`、`failed` 或 `timedOut`。`Pass` 表示提供方确认该请求不适用于自己。使用普通 NOF `Result` 或 `Result<T>` 的处理器返回 `Result.Fail(QuantumRequestContext.PassCode, ...)` 表达 Pass。提供方可直接返回实现 NOF `IResult` 的 `QuantumRequestResult<T>`，通过 `Answer(value)`、`Pass()` 和 `AnswerAndStop(value)` 表达行为。最后一种要求 `Ordered` 且声明 `AllowStop = true`。宿主使用同一个五秒截止时间处理整次插件请求。

`[RequestCapability("calendar.delete", QuantumDispatch.Ordered, UniqueOwner = true)]` 声明只允许唯一归属方写入。提供方自行决定请求是否适用于自己。链遇到第一项非 Pass 即停止。新增资源尚无 ID 时可以声明 `RequireSingleProvider`，有多个候选方时宿主会拒绝写入。

在 `plugin.json` 的 `capabilities.provides` 中列出能力名。Notify 的 `capabilities.notifications` 声明 `messageSchema`、可选的 `dispatch`（默认 `all`）和 `stopObservers`。.NET 用 `IQuantumNotifier.Observe()` 或 Web 用 `context.notify.subscribe()` 注册可停止的观察者。观察者异常会被记录，其他观察者仍会收到通知。通知在本机内存投递。

`RequestSingleAsync<T>()` 可将 Request 报告投影为单个结果。它拒绝多个非 Pass 结果。SDK 的 Roslyn Analyzer 检查缺失的声明（QTM001）、无效的分发策略（QTM002）、不支持的返回类型（QTM003），以及 Handler 直接调用 `AnswerAndStop` 时缺少 `AllowStop` 的情况（QTM004）。

.NET 和 Web 都能发起 Request 和发布或观察 Notify。.NET 插件通过 NOF `IRpcService`、`[TransportOverQuantum]` 和生成的 `RpcServer<T>` 提供 Request 处理器。

## 插件 ID、版本和版本范围

.NET 和 TypeScript 的 SDK 接口直接接收和返回字符串。可以直接调用 `environment.IsPluginLoaded("quantum.plugin.theme")`。宿主在读取插件清单时会规范化插件 ID，并校验版本和依赖范围。

`PluginId.Of`、`SemanticVersion.Of` 和 `VersionRange.Of` 是 .NET 的可选校验函数，返回普通字符串。用 `SemanticVersion.Compare(left, right)` 比较 SemVer 优先级，用 `SemanticVersion.Components(version)` 读取版本各部分。构建元数据不参与优先级比较。`VersionRange.Contains(range, version)` 支持有界和无界区间、有限集合及 `|` 并集。

数据库 schema 演进不属于 .NET ABI。插件可以在开发期使用 EF/NOF 模型，但发布包统一通过
`plugin.json` 的 `database.migrations` 携带 SQLite SQL artifact；Host 在 `StartAsync` 前应用它。可参考仓库中的插件示例。

## Notify 的 JSON 载荷

`IQuantumNotifier.Subscribe` 和 `Observe` 统一接收 `QuantumNotification`。订阅方可以直接读取 `Payload`（`JsonElement`），也可以调用 `TryDeserialize<T>()` 转成自己的 DTO。Web 的 `context.notify.subscribe` 同样接收原始 JSON，载荷类型为 `unknown`。

迁移已有的主题时，将能力名加入发布方清单的 `capabilities.provides`，并在 `capabilities.notifications` 声明其 `messageSchema`。用 `PublishAsync` / `context.notify.publish` 发送。观察者异常由 Host 记录，其他观察者继续接收。停止插件时释放订阅。

```csharp
await using var subscription = notify.Subscribe("devices.status", (notification, token) =>
{
    if (notification.TryDeserialize<DeviceStatus>(out var status) && status is not null)
    {
        logger.LogInformation("Device state is {State}", status.State);
    }

    return Task.CompletedTask;
});

await notify.PublishAsync("devices.status", new { state = "ready" }, cancellationToken);
```

## 安装插件

通过 `IQuantumRequester` 使用 `InstallMarketplacePluginAsync`、`InstallLocalPluginAsync` 和 `GetPluginInstallStatusAsync`。宿主征得用户同意后返回 `pending` 和操作 ID。保存 ID 并让当前处理器返回，同一插件可在重载后查询结果。

参见[移动界面](mobile-ui.md)和[术语表](glossary.md)。
