# Package verification and release

[简体中文](../zh-CN/packages.md)

The compatibility surface includes the .NET assembly identity and public types, the Web package exports and type declarations, and the behavior and serialized data shared by both runtimes. For pre-1.0 releases, select versions using the compatibility requirements of the host. The release tag `v<version>` identifies an immutable version shared by both packages. Update package metadata and release notes together. Use a protected, signed annotated tag on a verified commit.

Build the .NET package in Release mode. Pass the candidate version explicitly and use a new output directory for each run. In the commands below, replace `0.3.0-rc.2` with the version being evaluated.

```sh
dotnet pack dotnet/src/Quantum.Plugin.Abstraction/Quantum.Plugin.Abstraction.csproj -c Release -p:PackageVersion=0.3.0-rc.2 -p:ContinuousIntegrationBuild=true -o artifacts/sdk
python .ci/verify-sdk-package.py artifacts/sdk
dotnet build samples/Quantum.ExamplePlugin/Quantum.ExamplePlugin.csproj -c Release -p:QuantumSdkPackageVersion=0.3.0-rc.2 -p:RestoreAdditionalProjectSources="$PWD/artifacts/sdk"
dotnet build samples/Quantum.ExampleDependentPlugin/Quantum.ExampleDependentPlugin.csproj -c Release -p:QuantumSdkPackageVersion=0.3.0-rc.2 -p:RestoreAdditionalProjectSources="$PWD/artifacts/sdk"
npm pack --dry-run ./typescript
```

Build the TypeScript package with the commands in the [development guide](README.md) before inspecting its exports. Verify both package contents and a compatible host's runtime behavior. The package verifier limits the NuGet payload to its contract assembly, Roslyn analyzer, package manifest, license, notice and README. The internal mobile transport, application hosts and tests stay outside both packages.

Only publish approved artifacts through the release pipeline. Compare the candidate against versions already in each registry, publish the exact verified artifacts, and record the version, source commit, checksums, compatibility and package install verification in the release notes. If a published version needs correction, publish a new version. Consumers can pin the previous compatible version while upgrading.

GitHub Actions uses `publish.yml` and the `release` environment. Configure trusted publishers for both registries and set the NuGet.org profile name in the repository Actions variable `NUGET_USER`.
