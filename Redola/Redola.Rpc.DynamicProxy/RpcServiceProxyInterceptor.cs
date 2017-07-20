using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy
{
    public class RpcServiceProxyInterceptor : IInterceptor
    {
        private Type _serviceType;
        private string _serviceActorType;
        private MethodInfo _sendMethod;

        public RpcServiceProxyInterceptor(Type serviceType, string serviceActorType)
        {
            _serviceType = serviceType;
            _serviceActorType = serviceActorType;

            RetrieveSendMethod();
        }

        private void RetrieveSendMethod()
        {
            _sendMethod = typeof(RpcService).GetMethods()
                .Where(m => m.Name == "Send" && m.IsGenericMethod)
                .Where(m => m.ReturnType != typeof(void) && !m.ReturnType.IsGenericType)
                .Where(m => m.GetParameters().Count() == 2)
                .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(string)))
                .First();
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name == "RegisterRpcMessageContracts")
            {
                invocation.ReturnValue = BuildRpcMessageContracts();
            }
            else if (_serviceType.GetMethods().Select(m => m.Name).Contains(invocation.Method.Name))
            {
                invocation.ReturnValue = InvokeRpcMethod(invocation);
            }
            else
            {
                invocation.Proceed();
            }
        }

        private List<RpcMessageContract> BuildRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            var methods = _serviceType.GetMethods();
            foreach (var method in methods)
            {
                var methodParameters = method.GetParameters();
                if (methodParameters.Any())
                {
                    if (method.ReturnType == typeof(void))
                    {
                        messages.Add(new ReceiveMessageContract(methodParameters.First().ParameterType));
                    }
                    else
                    {
                        messages.Add(new RequestResponseMessageContract(methodParameters.First().ParameterType, method.ReturnType));
                    }
                }
            }

            return messages;
        }

        private object InvokeRpcMethod(IInvocation invocation)
        {
            var rpcMethod = _serviceType.GetMethods().First(m => m.Name == invocation.Method.Name);
            var rpcMethodParameter = rpcMethod.GetParameters().First();

            var genericSendMethod = _sendMethod.MakeGenericMethod(new Type[] { rpcMethodParameter.ParameterType, rpcMethod.ReturnType });

            var sendArguments = new List<object>();
            sendArguments.Add(_serviceActorType);
            sendArguments.AddRange(invocation.Arguments);

            return genericSendMethod.Invoke(invocation.Proxy, sendArguments.ToArray());
        }
    }
}
