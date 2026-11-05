# Quantum SDK Repository Instructions

- Follow the [English development standards](.standards/docs/en/README.md) at the commit recorded by this repository. Use the contributor-approved Git identity for both author and committer; never invent a placeholder identity.

- Start with the [English SDK documentation](docs/en/README.md).
- Keep English and Simplified Chinese documentation in separate, cross-linked files under `docs/en/` and `docs/zh-CN/`.
- Organize source by technical responsibility and mirror feature folders in tests. Keep verification helpers in `.ci/`.
- Verify public package contents. Keep host implementations and internal mobile transport outside the SDK.

## Capability parity

- Treat the .NET and TypeScript SDKs as adapters over the same Quantum plugin capability model.
- When changing a public capability in either runtime, inspect and update the other SDK and the Quantum Host adapter.
- Keep names, lifecycle behavior, validation, payloads, errors, cancellation, and cleanup aligned where possible.
- Update contract tests and developer documentation for both runtimes. Document intentional runtime differences.
- TypeScript declarations alone are not a complete Web capability; the matching iframe/Host transport must exist in Quantum.
