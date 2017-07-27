namespace Redola.Rpc
{
    public interface IServiceProxyGenerator
    {
        T CreateServiceProxy<T>(RpcHandler handler);
        T CreateServiceProxy<T>(RpcHandler handler, IServiceLoadBalancingStrategy strategy);
    }
}
