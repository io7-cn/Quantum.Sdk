import type { QuantumCallOptions } from "../runtime/types.js";

export type QuantumRequestContext = Readonly<Record<string, unknown>>;

export type QuantumResult<TResponse = undefined> =
  | {
      readonly isSuccess: true;
      readonly value: TResponse;
      readonly errorCode: string;
      readonly message: string;
      readonly extra?: Readonly<Record<string, string>> | null;
    }
  | {
      readonly isSuccess: false;
      readonly errorCode: string;
      readonly message: string;
      readonly extra?: Readonly<Record<string, string>> | null;
    };

export interface QuantumRequestOutcome<T> {
  readonly providerPluginId: string;
  readonly status: "handled" | "pass" | "failed" | "timedOut";
  readonly value: T | null;
  readonly hasValue: boolean;
  readonly errorCode: string | null;
  readonly message: string | null;
  readonly extra?: Readonly<Record<string, string>> | null;
}

export interface QuantumRequestReport<T> {
  readonly outcomes: readonly QuantumRequestOutcome<T>[];
  readonly stopped: boolean;
}

export interface QuantumRequester {
  <T>(capabilityName: string, payload: unknown, context?: QuantumRequestContext, options?: QuantumCallOptions):
    Promise<QuantumResult<QuantumRequestReport<T>>>;
}
