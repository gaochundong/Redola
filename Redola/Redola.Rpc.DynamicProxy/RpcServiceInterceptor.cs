using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy
{
    public class RpcServiceInterceptor : IInterceptor
    {
        private Type _targetType;

        public RpcServiceInterceptor(Type targetType)
        {
            _targetType = targetType;
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name == "RegisterRpcMessageContracts")
            {
                invocation.ReturnValue = BuildRpcMessageContracts();
            }
            else if (_targetType.GetMethods().Select(m => m.Name).Contains(invocation.Method.Name))
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

            var methods = _targetType.GetMethods();
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
            var rpcMethod = _targetType.GetMethods().First(m => m.Name == invocation.Method.Name);
            var rpcMethodParameter = rpcMethod.GetParameters().First();

            var sendMethods = typeof(RpcService).GetMethods().Where(m => m.Name == "Send");
            var sendMethod = sendMethods.First(m => m.IsGenericMethod && m.GetCustomAttribute<RpcServiceSendAttribute>() != null);
            var genericSendMethod = sendMethod.MakeGenericMethod(new Type[] { rpcMethodParameter.ParameterType, rpcMethod.ReturnType });

            var sendArguments = new List<object>();
            sendArguments.Add("server");
            sendArguments.AddRange(invocation.Arguments);

            return genericSendMethod.Invoke(invocation.Proxy, sendArguments.ToArray());
        }
    }
}
