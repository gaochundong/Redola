using System;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyGenerator : IServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        private MethodLocatorExtractor _extractor;
        private RpcHandler _rpcHandler;
        private IServiceResolver _serviceResolver;
        private IServiceLoadBalancingStrategy _strategy;

        public ServiceProxyGenerator(
            MethodLocatorExtractor extractor,
            RpcHandler rpcHandler,
            IServiceResolver serviceResolver,
            IServiceLoadBalancingStrategy strategy)
        {
            _extractor = extractor;
            _rpcHandler = rpcHandler;
            _serviceResolver = serviceResolver;
            _strategy = strategy;
        }

        public T CreateServiceProxy<T>(RpcHandler handler)
        {
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                new ProxyGenerationOptions(),
                new IInterceptor[]
                {
                    new ServiceProxyInterceptor(typeof(T), _extractor, _rpcHandler, _serviceResolver, _strategy)
                });
            return (T)proxy;
        }

        public T CreateServiceProxy<T>(RpcHandler handler, IServiceLoadBalancingStrategy strategy)
        {
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                new ProxyGenerationOptions(),
                new IInterceptor[]
                {
                    new ServiceProxyInterceptor(typeof(T), _extractor, _rpcHandler, _serviceResolver, strategy)
                });
            return (T)proxy;
        }
    }
}
