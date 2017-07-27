using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyGenerator : IServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        private MethodLocatorExtractor _locatorExtractor;
        private IServiceResolver _serviceResolver;

        public ServiceProxyGenerator(MethodLocatorExtractor locatorExtractor, IServiceResolver serviceResolver)
        {
            _locatorExtractor = locatorExtractor;
            _serviceResolver = serviceResolver;
        }

        public T CreateServiceProxy<T>(RpcHandler handler)
        {
            return CreateServiceProxy<T>(handler, new RandomServiceLoadBalancingStrategy());
        }

        public T CreateServiceProxy<T>(RpcHandler handler, IServiceLoadBalancingStrategy strategy)
        {
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                new ProxyGenerationOptions(),
                new IInterceptor[]
                {
                    new ServiceProxyInterceptor(typeof(T), _locatorExtractor, handler, _serviceResolver, strategy)
                });
            return (T)proxy;
        }
    }
}
