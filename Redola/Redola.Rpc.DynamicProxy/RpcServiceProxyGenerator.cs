using System;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy
{
    public static class RpcServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        public static T CreateServiceProxy<T>(RpcActor localActor, string serviceActorType)
        {
            var proxy = _proxyGenerator.CreateClassProxy(
                typeof(RpcService),
                new Type[] { typeof(T) },
                new ProxyGenerationOptions(),
                new object[] { localActor },
                new IInterceptor[] { new RpcServiceInterceptor(typeof(T), serviceActorType) });
            return (T)proxy;
        }
    }
}
