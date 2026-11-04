import type { QuantumPluginInfo } from "../manifest/identity.js";
import type { MaybePromise, QuantumCallOptions } from "../runtime/types.js";

export interface QuantumNotification {
  readonly id: string;
  readonly capabilityName: string;
  readonly payload: unknown;
  readonly publisher: QuantumPluginInfo;
  readonly publishedAt: string;
}

export interface QuantumNotificationSubscription {
  readonly capabilityName: string;
  dispose(): Promise<void>;
}

export interface QuantumNotifier {
  publish<TMessage>(capabilityName: string, payload: TMessage, options?: QuantumCallOptions): Promise<void>;
  subscribe(
    capabilityName: string,
    handler: (notification: QuantumNotification) => MaybePromise<void | "continue" | "stop">,
    options?: QuantumCallOptions & { readonly order?: number }
  ): Promise<QuantumNotificationSubscription>;
}
