import type { QuantumRequester, QuantumResult } from "../capabilities/request.js";
import type { QuantumCallOptions } from "../runtime/types.js";

export const installMarketplacePluginCapabilityName = "quantum.plugins.installmarketplace";
export const installLocalPluginCapabilityName = "quantum.plugins.installlocal";
export const getPluginInstallStatusCapabilityName = "quantum.plugins.installstatus";

export interface QuantumPluginInstallReceipt {
  readonly operationId: string;
  readonly status: "pending" | "succeeded" | "failed";
  readonly message?: string | null;
}

/** Requests user approval to install the compatible Marketplace release. */
export function installMarketplacePlugin(
  context: { readonly request: QuantumRequester }, pluginId: string, options?: QuantumCallOptions
): Promise<QuantumResult<QuantumPluginInstallReceipt>> {
  return requestInstallation(context, installMarketplacePluginCapabilityName, { pluginId }, options);
}

/** Requests user approval to install a ZIP package supplied as standard base64. */
export function installLocalPlugin(
  context: { readonly request: QuantumRequester }, archiveFileName: string, archiveBase64: string, options?: QuantumCallOptions
): Promise<QuantumResult<QuantumPluginInstallReceipt>> {
  return requestInstallation(context, installLocalPluginCapabilityName, { archiveFileName, archiveBase64 }, options);
}

export function getPluginInstallStatus(
  context: { readonly request: QuantumRequester }, operationId: string, options?: QuantumCallOptions
): Promise<QuantumResult<QuantumPluginInstallReceipt>> {
  return requestInstallation(context, getPluginInstallStatusCapabilityName, { operationId }, options);
}

async function requestInstallation(
  context: { readonly request: QuantumRequester }, name: string, payload: unknown, options?: QuantumCallOptions
): Promise<QuantumResult<QuantumPluginInstallReceipt>> {
  const result = await context.request<QuantumPluginInstallReceipt>(name, payload, {}, options);
  if (!result.isSuccess) {
    return result;
  }

  const [outcome] = result.value.outcomes;
  if (result.value.outcomes.length === 1 && outcome?.status === "handled" && outcome.value !== null) {
    return { ...result, value: outcome.value };
  }

  return {
    isSuccess: false,
    errorCode: outcome?.errorCode ?? "capability_installation_unavailable",
    message: outcome?.message ?? "Plugin installation is unavailable.",
    extra: outcome?.extra ?? null
  };
}
