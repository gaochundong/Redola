using System;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public static class RpcServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        public static T CreateServiceProxy<T>(RpcActor localActor, string serviceActorType)
        {
            var proxy = _proxyGenerator.CreateClassProxy(
                typeof(RpcHandler),
                new Type[] { typeof(T) },
                new ProxyGenerationOptions(),
                new object[] { localActor },
                new IInterceptor[] { new RpcServiceProxyInterceptor(typeof(T), serviceActorType) });
            return (T)proxy;
        }
    }
}
