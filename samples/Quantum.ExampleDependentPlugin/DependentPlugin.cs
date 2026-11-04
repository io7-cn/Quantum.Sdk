using Microsoft.Extensions.DependencyInjection;
using NOF.Contract;
using Quantum.Plugin.Abstraction;

namespace Quantum.ExampleDependentPlugin;

/// <summary>
/// Demonstrates that a manifest dependency is available before this plugin starts.
/// </summary>
public sealed class DependentPlugin : IQuantumPlugin
{
    private static readonly PluginId _requiredPluginId = PluginId.Of("quantum.plugin.example");
    private const string GreetingRpcName = "quantum.plugin.example.example.greet";

    public static async Task StartAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IQuantumPluginEnvironment environment = services.GetRequiredService<IQuantumPluginEnvironment>();
        QuantumPluginInfo? requiredPlugin = environment.LoadedPlugins.SingleOrDefault(plugin =>
            plugin.Id == _requiredPluginId);
        if (requiredPlugin is null)
        {
            throw new InvalidOperationException(
                $"Required plugin '{_requiredPluginId}' was not loaded before the dependent example.");
        }

        Result<string> greetingResult = await services
            .GetRequiredService<IRpcInvoker>()
            .InvokeAsync<string>(
                GreetingRpcName,
                new Empty(),
                Context.Empty,
                cancellationToken)
            .ConfigureAwait(false);
        if (!greetingResult.IsSuccess)
        {
            throw new InvalidOperationException(
                $"RPC '{GreetingRpcName}' failed: {greetingResult.ErrorCode} {greetingResult.Message}");
        }

        services.GetRequiredService<DependentPluginState>().Start(
            DateTimeOffset.Now,
            requiredPlugin,
            greetingResult.Value);
    }

    public static Task StopAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        services.GetRequiredService<DependentPluginState>().Stop();
        return Task.CompletedTask;
    }
}
