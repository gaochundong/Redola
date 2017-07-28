namespace Redola.Rpc
{
    public interface IServiceProxyGenerator
    {
        T CreateServiceProxy<T>(RpcHandler handler, RpcMethodFixture fixture);
        T CreateServiceProxy<T>(RpcHandler handler, RpcMethodFixture fixture, IServiceLoadBalancingStrategy strategy);
    }
}
