import type { QuantumDispatch } from "../capabilities/dispatch.js";

/** The capabilities section of plugin.json. Entries are capability names. */
export interface PluginCapabilities {
  /** Web plugins can provide Notify capabilities. Each provided name needs a notification contract. */
  readonly provides?: readonly string[];
  readonly notifications?: Readonly<Record<string, {
    readonly description?: string | null;
    readonly messageSchema: Readonly<Record<string, unknown>>;
    readonly dispatch?: QuantumDispatch;
    readonly stopObservers?: readonly string[];
  }>>;
  readonly requires?: readonly string[];
  readonly integrates?: readonly string[];
}
