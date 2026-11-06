import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

test("extension exports use JSON contracts without generic business types", async () => {
  const types = await readFile(new URL("../dist/extensions/index.d.ts", import.meta.url), "utf8");
  const context = await readFile(new URL("../dist/runtime/plugin.d.ts", import.meta.url), "utf8");
  const index = await readFile(new URL("../dist/index.d.ts", import.meta.url), "utf8");
  assert.match(index, /extensions\/index/);
  assert.match(context, /readonly extensions: QuantumExtensions/);
  assert.match(types, /value: ExtensionJson/);
  assert.match(types, /define\(localId: string, contractVersion: number\)/);
  assert.match(types, /observe\(pointId: string, contractVersion: number/);
  assert.doesNotMatch(types, /<T[>, ]/);
});
