using System;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy
{
    public static class RpcServiceProxyGenerator
    {
        private static readonly IProxyGenerator _proxyGenerator = new ProxyGenerator();

        public static T CreateServiceProxy<T>(params object[] serviceArguments)
        {
            var proxy = _proxyGenerator.CreateClassProxy(
                typeof(RpcService),
                new Type[] { typeof(T) },
                new ProxyGenerationOptions(),
                serviceArguments,
                new IInterceptor[] { new RpcServiceInterceptor(typeof(T)) });
            return (T)proxy;
        }
    }
}
