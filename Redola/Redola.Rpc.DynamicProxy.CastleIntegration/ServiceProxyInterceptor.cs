using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyInterceptor : IInterceptor
    {
        private Type _serviceType;
        private RpcHandler _rpcHandler;
        private MethodLocatorExtractor _extractor;

        public ServiceProxyInterceptor(Type serviceType, RpcHandler rpcHandler, MethodLocatorExtractor extractor)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (rpcHandler == null)
                throw new ArgumentNullException("rpcHandler");
            if (extractor == null)
                throw new ArgumentNullException("extractor");

            _serviceType = serviceType;
            _rpcHandler = rpcHandler;
            _extractor = extractor;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.ReturnType == typeof(void))
            {
                InvokeRpcMethod(invocation);
            }
            else
            {
                invocation.ReturnValue = InvokeRpcMethodReturn(invocation);
            }
        }

        private void InvokeRpcMethod(IInvocation invocation)
        {
            var rpcMethod = _serviceType.GetType()
                .GetMethod(invocation.Method.Name, invocation.Method.GetParameters().Select(p => p.ParameterType).ToArray());

            var methodLocator = _extractor.Extract(rpcMethod);

            var message = new InvokeMethodMessage()
            {
                MethodLocator = methodLocator,
                MethodArguments = invocation.Arguments,
            };

            _rpcHandler.Send("", message);
        }

        private object InvokeRpcMethodReturn(IInvocation invocation)
        {
            var rpcMethod = _serviceType.GetType()
                .GetMethod(invocation.Method.Name, invocation.Method.GetParameters().Select(p => p.ParameterType).ToArray());

            var methodLocator = _extractor.Extract(rpcMethod);

            var request = new InvokeMethodRequest()
            {
                MethodLocator = methodLocator,
                MethodArguments = invocation.Arguments,
            };

            var response = _rpcHandler.Send<InvokeMethodRequest, InvokeMethodResponse>("", request);

            return response.MethodReturnValue;
        }
    }
}
