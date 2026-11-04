import type { QuantumCapabilities } from "../capabilities/catalog.js";
import type { QuantumNotifier } from "../capabilities/notify.js";
import type { QuantumRequester } from "../capabilities/request.js";
import type { QuantumPluginIdentity, QuantumPluginInfo } from "../manifest/identity.js";
import type { QuantumMobilePages } from "../mobile/index.js";
import type { MaybePromise, QuantumCallOptions, QuantumCleanup } from "./types.js";

export interface QuantumPluginIntegration {
  readonly pluginId: string;
  readonly versionRange: string;
  /** Informational availability of the declared target; this does not gate plugin interaction. */
  readonly active: boolean;
}

export interface QuantumEnvironmentSnapshot {
  readonly plugin: QuantumPluginIdentity;
  readonly loadedPlugins: readonly QuantumPluginInfo[];
  readonly integrations: readonly QuantumPluginIntegration[];
}

export interface QuantumPluginContext {
  readonly plugin: QuantumPluginIdentity;
  /** Aborted before plugin cleanup runs. */
  readonly signal: AbortSignal;
  readonly log: {
    trace(message: string): Promise<void>;
    debug(message: string): Promise<void>;
    info(message: string): Promise<void>;
    warn(message: string): Promise<void>;
    error(message: string): Promise<void>;
  };
  readonly navigation: {
    navigate(href: string): Promise<void>;
  };
  readonly environment: {
    snapshot(): Promise<QuantumEnvironmentSnapshot>;
  };
  readonly assets: {
    url(path: string): string;
    readText(path: string, options?: QuantumCallOptions): Promise<string>;
  };
  readonly capabilities: QuantumCapabilities;
  readonly notify: QuantumNotifier;
  readonly request: QuantumRequester;
  readonly mobile: QuantumMobilePages;
}

export interface QuantumRoute {
  readonly path: string;
  readonly view: string;
  readonly title?: string | null;
}

/** Locale selected by the Host for the mounted plugin view. */
export interface QuantumLocale {
  /** Normalized BCP-47 culture name, such as `en-US` or `zh-CN`. */
  readonly cultureName: string;
  /** ISO language name, such as `en` or `zh`. */
  readonly languageName: string;
  readonly textDirection: "ltr" | "rtl";
}

export interface QuantumPluginViewContext extends Omit<QuantumPluginContext, "signal"> {
  /** The root element inside the plugin's isolated document. */
  readonly element: HTMLElement;
  readonly route: QuantumRoute;
  readonly locale: QuantumLocale;
  /** Aborted whenever the view is unmounted. */
  readonly signal: AbortSignal;
}

export interface QuantumPluginDefinition {
  activate?(context: QuantumPluginContext): MaybePromise<QuantumCleanup>;
  deactivate?(context: QuantumPluginContext): MaybePromise<void>;
  mount?(context: QuantumPluginViewContext): MaybePromise<QuantumCleanup>;
  unmount?(context: QuantumPluginContext): MaybePromise<void>;
}

/**
 * Adds compile-time checking while leaving the plugin definition unchanged for bundlers.
 */
export function definePlugin<TDefinition extends QuantumPluginDefinition>(
  definition: TDefinition
): TDefinition {
  return definition;
}
