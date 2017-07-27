using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyInterceptor : IInterceptor
    {
        private Type _serviceType;
        private MethodLocatorExtractor _extractor;
        private RpcHandler _rpcHandler;
        private IServiceResolver _serviceResolver;
        private IServiceLoadBalancingStrategy _strategy;

        public ServiceProxyInterceptor(
            Type serviceType,
            MethodLocatorExtractor extractor,
            RpcHandler rpcHandler,
            IServiceResolver serviceResolver,
            IServiceLoadBalancingStrategy strategy)
        {
            _serviceType = serviceType;
            _extractor = extractor;
            _rpcHandler = rpcHandler;
            _serviceResolver = serviceResolver;
            _strategy = strategy;
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

            var service = _serviceResolver.Resolve(_serviceType, _strategy);
            _rpcHandler.Send(service, message);
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

            var service = _serviceResolver.Resolve(_serviceType, _strategy);
            var response = _rpcHandler.Send<InvokeMethodRequest, InvokeMethodResponse>(service, request);

            return response.MethodReturnValue;
        }
    }
}
