export type ExtensionJson = null | boolean | number | string | readonly ExtensionJson[] |
  { readonly [key: string]: ExtensionJson };

export interface QuantumExtensionContribution {
  readonly pluginId: string;
  readonly localId: string;
  readonly order: number;
  readonly value: ExtensionJson;
}

export interface QuantumExtensionSnapshot {
  readonly pointId: string;
  readonly contractVersion: number;
  readonly revision: number;
  readonly isAvailable: boolean;
  readonly contributions: readonly QuantumExtensionContribution[];
}

export interface QuantumExtensionRegistration {
  dispose(): Promise<void>;
}

export interface QuantumContribution extends QuantumExtensionRegistration {
  update(value: ExtensionJson): Promise<void>;
}

/** Runtime-scoped registrations. The host supplies identity and releases all handles on stop. */
export interface QuantumExtensions {
  define(localId: string, contractVersion: number): Promise<QuantumExtensionRegistration>;
  register(pointId: string, contractVersion: number, localId: string,
    value: ExtensionJson, order?: number): Promise<QuantumContribution>;
  observe(pointId: string, contractVersion: number,
    observer: (snapshot: QuantumExtensionSnapshot) => void): Promise<QuantumExtensionRegistration>;
}
