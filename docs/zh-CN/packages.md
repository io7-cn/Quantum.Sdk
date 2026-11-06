# 包验证与发布

[English](../en/packages.md)

兼容范围包括 .NET 程序集标识和公开类型、Web 包的导出和类型声明，以及两个运行时共享的行为和序列化数据。对 1.0 之前的版本，根据宿主的兼容性要求选择包版本。发布标签 `v<version>` 标识两个包共同使用的不可变版本。同步更新包元数据和发布说明，在已验证的提交上使用受保护的签名附注标签。

用 Release 配置构建 .NET 包。显式传入候选版本，每次运行使用新的输出目录。执行下列命令时，将 `0.3.0-rc.2` 替换为待验证的版本。

```sh
dotnet pack dotnet/src/Quantum.Plugin.Abstraction/Quantum.Plugin.Abstraction.csproj -c Release -p:PackageVersion=0.3.0-rc.2 -p:ContinuousIntegrationBuild=true -o artifacts/sdk
python .ci/verify-sdk-package.py artifacts/sdk
dotnet build samples/Quantum.ExamplePlugin/Quantum.ExamplePlugin.csproj -c Release -p:QuantumSdkPackageVersion=0.3.0-rc.2 -p:RestoreAdditionalProjectSources="$PWD/artifacts/sdk"
dotnet build samples/Quantum.ExampleDependentPlugin/Quantum.ExampleDependentPlugin.csproj -c Release -p:QuantumSdkPackageVersion=0.3.0-rc.2 -p:RestoreAdditionalProjectSources="$PWD/artifacts/sdk"
npm pack --dry-run ./typescript
```

检查 TypeScript 包的导出前，先按[开发指南](README.md)构建。验证两个包的内容，以及在兼容宿主中的实际运行行为。NuGet 包检查限定了允许发布的内容，包括契约程序集、Roslyn 分析器、包清单、许可证、版权声明和 README。内部移动传输、应用宿主和测试均留在两个包之外。

通过发布流水线发布已批准的产物。对照各注册表的现有版本检查候选版本，发布已验证的原始产物，在发布说明中记录版本、源提交、校验和、兼容性和安装验证结果。需要修正已发布版本时，发布新版本。使用方可固定在上一兼容版本，直至完成升级。

GitHub Actions 使用 `publish.yml` 和 `release` 环境。为两个注册表配置可信发布，并在仓库 Actions 变量 `NUGET_USER` 中填写 NuGet.org 用户名。

## 插件兼容版本

插件兼容性以 SDK 发布版本为依据，与 Quantum Desktop 应用版本独立。.NET 和 TypeScript 包共享该发布版本。宿主上报所引用 `Quantum.Plugin.Abstraction` 程序集的信息版本，保留预发布标识并排除构建元数据。发布插件时显式提供 `--sdk-version-support` 范围，例如 `"0.3.0-rc.2"` 表示精确预发布版本，`">=0.3.0-rc.2 <0.4.0"` 表示已测试的范围。预发布版本优先级低于对应的稳定版；`0.3.0-rc.2` 不满足 `>=0.3.0`。在支持的两个运行时中验证声明的范围。插件间依赖范围仍指向被依赖插件自身的版本。
