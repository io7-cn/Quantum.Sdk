using NOF.Contract;
using System.Text.Json;
using Xunit;

namespace Quantum.Plugin.Abstraction.Tests;

public sealed class PluginRpcContractTests
{
    [Fact]
    public void RpcInvokerIsAnInterface()
    {
        Assert.True(typeof(IQuantumRequester).IsInterface);
        Assert.Equal(
            1,
            typeof(IQuantumRequester).GetMethods()
                .Count(static method => method.Name == nameof(IQuantumRequester.RequestAsync)));
    }

    [Fact]
    public void TransportGeneratesOnlyTheProtocolNeutralClientInterface()
    {
        Assert.True(typeof(IRpcProbeServiceClient).IsInterface);
        Assert.Contains(
            typeof(IRpcProbeServiceClient).GetInterfaces(),
            static implemented => implemented == typeof(IRpcClient<IRpcProbeService>));
        Assert.DoesNotContain(
            typeof(PluginRpcContractTests).Assembly.GetTypes(),
            static type => type.Name is "HttpRpcProbeServiceClient"
                or "LocalRpcProbeServiceClient"
                or "QuantumRpcProbeServiceClient");
    }

    [Theory]
    [InlineData("probe")]
    [InlineData("PROBE.ping")]
    [InlineData("probe..ping")]
    [InlineData("probe.ping\n")]
    [InlineData("quantum.rpc.catalog")]
    public void CapabilityDeclarationsAndManifestsRejectTheSameInvalidNames(string name)
    {
        Assert.Throws<ArgumentException>(() => new RequestCapabilityAttribute(name, QuantumDispatch.All));
        Assert.Throws<ArgumentException>(() => new PluginCapabilities(provides: [name]));
    }

    [Fact]
    public async Task CatalogHelperUsesTheBuiltInDiscoveryRpc()
    {
        var invoker = new RecordingRpcInvoker();

        var result = await invoker.DiscoverCapabilitiesAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(QuantumCapabilityCatalog.CapabilityName, invoker.RpcName);
        Assert.Equal(Context.Empty, invoker.Context);
        Assert.Equal("{}", JsonSerializer.Serialize(invoker.Payload));
    }

    private sealed class RecordingRpcInvoker : IQuantumRequester
    {
        public string? RpcName { get; private set; }

        public object? Payload { get; private set; }

        public Context? Context { get; private set; }

        public Task<Result<QuantumRequestReport<TResponse>>> RequestAsync<TResponse>(
            string rpcName,
            object payload,
            Context context,
            CancellationToken cancellationToken = default)
        {
            RpcName = rpcName;
            Payload = payload;
            Context = context;
            object catalog = typeof(TResponse) == typeof(QuantumPluginInstallReceipt)
                ? new QuantumPluginInstallReceipt("operation", "pending", null)
                : new QuantumCapabilityCatalog(2, []);
            return Task.FromResult(Result.Success(new QuantumRequestReport<TResponse>(
                [new("quantum.host", QuantumRequestStatus.Handled, (TResponse)catalog, null, null) { HasValue = true }], false)));
        }
    }

    [Fact]
    public async Task InstallationHelpersUseTheSharedWirePayloads()
    {
        var invoker = new RecordingRpcInvoker();
        var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        await invoker.InstallMarketplacePluginAsync("quantum.plugin.example");
        Assert.Equal("quantum.plugins.installmarketplace", invoker.RpcName);
        Assert.Equal("""{"pluginId":"quantum.plugin.example"}""", JsonSerializer.Serialize(invoker.Payload, json));
        await invoker.InstallLocalPluginAsync("example.zip", [0x50, 0x4b]);
        Assert.Equal("quantum.plugins.installlocal", invoker.RpcName);
        Assert.Equal("""{"archiveFileName":"example.zip","archiveBase64":"UEs="}""", JsonSerializer.Serialize(invoker.Payload, json));
        await invoker.GetPluginInstallStatusAsync("operation");
        Assert.Equal("quantum.plugins.installstatus", invoker.RpcName);
        Assert.Equal("""{"operationId":"operation"}""", JsonSerializer.Serialize(invoker.Payload, json));
    }
}

public sealed record RpcProbeRequest(string Value);

[TransportOverQuantum]
public interface IRpcProbeService : IRpcService
{
    [RequestCapability("probe.ping", QuantumDispatch.All)]
    Result<string> Ping(RpcProbeRequest request);
}
