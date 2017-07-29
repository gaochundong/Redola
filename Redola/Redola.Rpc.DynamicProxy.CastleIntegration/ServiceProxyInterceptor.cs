using System;
using System.Linq;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class ServiceProxyInterceptor : IInterceptor
    {
        private Type _serviceType;
        private IServiceResolver _serviceResolver;

        private RpcHandler _handler;
        private RpcMethodFixture _fixture;
        private IServiceLoadBalancingStrategy _strategy;

        public ServiceProxyInterceptor(
            Type serviceType,
            IServiceResolver serviceResolver,
            RpcHandler handler,
            RpcMethodFixture fixture,
            IServiceLoadBalancingStrategy strategy)
        {
            _serviceType = serviceType;
            _serviceResolver = serviceResolver;

            _handler = handler;
            _fixture = fixture;
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
            var rpcMethod = _serviceType.GetMethod(invocation.Method.Name, invocation.Method.GetParameters().Select(p => p.ParameterType).ToArray());
            if (rpcMethod == null)
                throw new InvalidOperationException(string.Format("Cannot invoke method [{0}].", invocation.Method.Name));

            var methodLocator = _fixture.Extractor.Extract(rpcMethod);

            var message = new InvokeMethodMessage()
            {
                MethodLocator = methodLocator,
                MethodArguments = invocation.Arguments,
            };

            message.Serialize(_fixture.ArgumentEncoder);

            var service = _serviceResolver.Resolve(_serviceType, _strategy);
            _handler.Send(service.Type, service.Name, message);
        }

        private object InvokeRpcMethodReturn(IInvocation invocation)
        {
            var rpcMethod = _serviceType.GetMethod(invocation.Method.Name, invocation.Method.GetParameters().Select(p => p.ParameterType).ToArray());
            if (rpcMethod == null)
                throw new InvalidOperationException(string.Format("Cannot invoke method [{0}].", invocation.Method.Name));

            var methodLocator = _fixture.Extractor.Extract(rpcMethod);

            var request = new InvokeMethodRequest()
            {
                MethodLocator = methodLocator,
                MethodArguments = invocation.Arguments,
            };

            request.Serialize(_fixture.ArgumentEncoder);

            var service = _serviceResolver.Resolve(_serviceType, _strategy);
            var response = _handler.Send<InvokeMethodRequest, InvokeMethodResponse>(service.Type, service.Name, request);

            response.Deserialize(_fixture.ArgumentDecoder);

            return response.MethodReturnValue;
        }
    }
}
