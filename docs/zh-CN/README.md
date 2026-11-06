# Quantum 插件 SDK

[English](../en/README.md)

本仓库为 Quantum Desktop 提供插件开发工具包。NuGet 包 `Quantum.Plugin.Abstraction` 定义公开的 .NET 插件契约，npm 包 `@io7/quantum-plugin-sdk` 定义实验性 Web 插件契约。开发插件时，请选择与目标宿主兼容的包版本，并在插件项目中固定版本。

| 目录 | 用途 |
| --- | --- |
| `dotnet/` | .NET 契约和测试 |
| `typescript/` | Web 契约和测试 |
| `samples/` | 可运行的 .NET 和 Web 插件示例 |
| `fixtures/` | 供两套 [SDK](glossary.md#sdk) 使用的共享测试数据 |
| `docs/` | 开发文档 |
| `.ci/` | 包和版本检查 |

安装 .NET 10 SDK、Node.js 22 和 npm，在仓库根目录执行：

```sh
dotnet test Quantum.Sdk.slnx -c Release
dotnet build Quantum.Sdk.slnx -c Release
npm ci --prefix typescript
npm test --prefix typescript
npm ci --prefix samples/Quantum.ExampleWebPlugin
npm test --prefix samples/Quantum.ExampleWebPlugin
```

运行 `dotnet format Quantum.Sdk.slnx`，按根目录的 `.editorconfig` 规则格式化 C#。C# 和 TypeScript 的控制流块都使用大括号，包括只有一条语句的分支。

参见 [.NET 插件开发](dotnet-plugin-development.md)、[Web 插件开发](web-plugin-development.md)、[运行时扩展](extensions.md)、[移动界面](mobile-ui.md)、[包验证与发布](packages.md)、[术语表](glossary.md)和[贡献指南](CONTRIBUTING.md)。SDK 由艾欧柒（杭州）智能科技有限公司的 Quantum 维护团队负责。一般问题请通过仓库的议题区反馈，漏洞请参照[安全报告说明](../../SECURITY.md)。项目采用 [Apache 2.0 许可证](../../LICENSE)。

## 开发规范

遵循[简体中文开发规范](../../.standards/docs/zh-CN/README.md)。.standards 子模块记录经过审查的确切版本；上游变更只有在显式更新该提交后才生效。开发前初始化：

```sh
git submodule update --init --recursive
python .standards/.ci/check-adoption.py
```

同步维护两种语言。AGENTS.md 指向采用的英文入口，项目专属指令补充共享规范。
