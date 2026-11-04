export type MaybePromise<T> = T | PromiseLike<T>;

export type QuantumCleanup =
  | void
  | (() => MaybePromise<void>)
  | { dispose(): MaybePromise<void> };

export interface QuantumCallOptions {
  signal?: AbortSignal;
}
