using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyGenerator : IServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        public T CreateServiceProxy<T>(RpcHandler handler)
        {
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                new ProxyGenerationOptions(),
                new IInterceptor[] { new ServiceProxyInterceptor(typeof(T), handler, new MethodLocatorExtractor()) });
            return (T)proxy;
        }
    }
}
