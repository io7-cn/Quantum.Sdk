import type { QuantumCallOptions } from "../runtime/types.js";
import type { QuantumDispatch } from "./dispatch.js";
import type { QuantumResult } from "./request.js";

export type QuantumCapabilityKind = "request" | "notify";

export interface QuantumCapabilityAttributeArgument {
  readonly type: string;
  readonly value: unknown;
}

export interface QuantumCapabilityAttribute {
  readonly type: string;
  readonly constructorArguments: readonly QuantumCapabilityAttributeArgument[];
  readonly namedArguments: Readonly<Record<string, QuantumCapabilityAttributeArgument>>;
}

export interface QuantumCapabilityDescription {
  readonly name: string;
  readonly kind: QuantumCapabilityKind;
  readonly contractVersion: number;
  readonly description: string | null;
  readonly providerPluginIds: readonly string[];
  readonly requestSchema: Readonly<Record<string, unknown>> | null;
  readonly responseSchema: Readonly<Record<string, unknown>> | null;
  readonly messageSchema: Readonly<Record<string, unknown>> | null;
  readonly attributes: readonly QuantumCapabilityAttribute[];
  readonly parameterAttributes: readonly QuantumCapabilityAttribute[];
  readonly returnAttributes: readonly QuantumCapabilityAttribute[];
  readonly dispatch: QuantumDispatch;
  readonly uniqueOwner: boolean;
  readonly requireSingleProvider: boolean;
  readonly allowStop: boolean;
}

export interface QuantumCapabilityCatalog {
  readonly schemaVersion: 2;
  readonly capabilities: readonly QuantumCapabilityDescription[];
}

export interface QuantumCapabilities {
  catalog(options?: QuantumCallOptions): Promise<QuantumResult<QuantumCapabilityCatalog>>;
}
