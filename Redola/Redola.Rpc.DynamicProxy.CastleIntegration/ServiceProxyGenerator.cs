using System;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyGenerator : IServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        private IServiceResolver _serviceResolver;

        public ServiceProxyGenerator(IServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
                throw new ArgumentNullException("serviceResolver");
            _serviceResolver = serviceResolver;
        }

        public T CreateServiceProxy<T>(RpcHandler handler, RpcMethodFixture fixture)
        {
            return CreateServiceProxy<T>(handler, fixture, new RandomServiceLoadBalancingStrategy());
        }

        public T CreateServiceProxy<T>(RpcHandler handler, RpcMethodFixture fixture, IServiceLoadBalancingStrategy strategy)
        {
            var proxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(
                typeof(T),
                new ProxyGenerationOptions(),
                new IInterceptor[]
                {
                    new ServiceProxyInterceptor(
                        typeof(T),
                        _serviceResolver,
                        handler,
                        fixture,
                        strategy)
                });
            return (T)proxy;
        }
    }
}
