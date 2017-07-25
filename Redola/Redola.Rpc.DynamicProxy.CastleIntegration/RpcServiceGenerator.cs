using System;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public static class RpcServiceGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        public static T CreateService<T>(RpcActor localActor, T service)
        {
            var proxy = _proxyGenerator.CreateClassProxy(
                typeof(RpcService),
                new Type[] { typeof(T) },
                new ProxyGenerationOptions(),
                new object[] { localActor },
                new IInterceptor[] { new RpcServiceInterceptor<T>(service) });
            return (T)proxy;
        }
    }
}
