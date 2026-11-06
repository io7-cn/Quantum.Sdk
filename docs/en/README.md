# Quantum Plugin SDK

[简体中文](../zh-CN/README.md)

Develop plugins for Quantum Desktop. The NuGet package `Quantum.Plugin.Abstraction` defines the public .NET plugin contracts. The npm package `@io7/quantum-plugin-sdk` defines the contracts for experimental Web plugins. Choose package versions compatible with your target host, and pin the versions in your plugin project.

| Directory | Purpose |
| --- | --- |
| `dotnet/` | .NET contracts and tests |
| `typescript/` | Web contracts and tests |
| `samples/` | Runnable .NET and Web plugin examples |
| `fixtures/` | Shared data for checking both SDKs |
| `docs/` | Developer documentation |
| `.ci/` | Package and version checks |

Install the .NET 10 [SDK](glossary.md#sdk), Node.js 22 and npm. From the repository root:

```sh
dotnet test Quantum.Sdk.slnx -c Release
dotnet build Quantum.Sdk.slnx -c Release
npm ci --prefix typescript
npm test --prefix typescript
npm ci --prefix samples/Quantum.ExampleWebPlugin
npm test --prefix samples/Quantum.ExampleWebPlugin
```

Run `dotnet format Quantum.Sdk.slnx` to apply the root `.editorconfig` rules to C#. Use braces in C# and TypeScript control blocks, including single statements.

See [.NET plugin development](dotnet-plugin-development.md), [Web plugin development](web-plugin-development.md), [Mobile UI](mobile-ui.md), [Package verification and release](packages.md), [Terminology](glossary.md) and [Contributing](CONTRIBUTING.md). The SDK is maintained by the Quantum maintainers at 艾欧柒（杭州）智能科技有限公司. Use the repository issue tracker for support and the [security reporting instructions](../../SECURITY.md) for vulnerabilities. Licensed under [Apache 2.0](../../LICENSE).

## Development standards

Follow the [English development standards](../../.standards/docs/en/README.md). The .standards submodule records the exact reviewed revision; upstream changes apply only after an explicit update of that recorded commit. Initialize it before development:

```sh
git submodule update --init --recursive
python .standards/.ci/check-adoption.py
```

Keep both language editions aligned. AGENTS.md links to the adopted English entry point; project-specific instructions supplement the shared requirements.

[Runtime extensions](extensions.md)
