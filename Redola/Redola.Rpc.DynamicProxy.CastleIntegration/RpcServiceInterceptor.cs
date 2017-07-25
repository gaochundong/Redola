using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy.CastleIntegration
{
    public class RpcServiceInterceptor<T> : IInterceptor
    {
        private T _service;
        private MethodInfo _replyMethod;

        public RpcServiceInterceptor(T service)
        {
            _service = service;

            RetrieveReplyMethod();
        }

        private void RetrieveReplyMethod()
        {
            _replyMethod = typeof(RpcService).GetMethods()
                .Where(m => m.Name == "Reply" && m.IsGenericMethod)
                .Where(m => m.ReturnType == typeof(void))
                .Where(m => m.GetParameters()
                    .Any(p => p.ParameterType.IsGenericType
                        && p.ParameterType.GetGenericTypeDefinition() == typeof(ActorMessageEnvelope<>)))
                .First();
        }

        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name == "RegisterRpcMessageContracts")
            {
                invocation.ReturnValue = BuildRpcMessageContracts();
            }
            else if (invocation.Method.Name == "DoHandleMessage")
            {
                InvokeRpcMethod(invocation);
            }
            else
            {
                invocation.Proceed();
            }
        }

        private List<RpcMessageContract> BuildRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            var methods = typeof(T).GetMethods();
            foreach (var method in methods)
            {
                var methodParameters = method.GetParameters();
                if (methodParameters.Any())
                {
                    messages.Add(new ReceiveMessageContract(methodParameters.First().ParameterType));
                }
            }

            return messages;
        }

        private void InvokeRpcMethod(IInvocation invocation)
        {
            var invokedSender = invocation.Arguments[0];
            var invokedEnvelope = invocation.Arguments[1];

            var sender = invokedSender.GetType();
            var channelIdentifier = (string)sender.GetProperty("ChannelIdentifier").GetValue(invokedSender);

            var envelope = invokedEnvelope.GetType();
            var messageType = (string)envelope.GetProperty("MessageType").GetValue(invokedEnvelope);

            var rpcMethod = typeof(T).GetMethods().First(m => m.GetParameters().Any(p => p.ParameterType.Name == messageType));
            var rpcMethodParameter = rpcMethod.GetParameters().First();

            var actorHandler = typeof(RouteActorMessageHandlerBase);
            var actor = (RouteActor)actorHandler.GetProperty("Actor").GetValue(invocation.Proxy);

            var instantiateMethod = typeof(ActorMessageEnvelopeExtensions)
                .GetMethod("Instantiate", new Type[] { typeof(ActorMessageEnvelope), typeof(IActorMessageDecoder) })
                .MakeGenericMethod(rpcMethodParameter.ParameterType);
            var instantiatedEnvelope = instantiateMethod.Invoke(null, new object[] { invokedEnvelope, actor.Decoder });

            var messageID = (string)instantiatedEnvelope.GetType().GetProperty("MessageID").GetValue(instantiatedEnvelope);
            var messageTime = (DateTime)instantiatedEnvelope.GetType().GetProperty("MessageTime").GetValue(instantiatedEnvelope);
            var messageRequest = instantiatedEnvelope.GetType().GetProperty("Message").GetValue(instantiatedEnvelope);

            var messageResponse = rpcMethod.Invoke(_service, new object[] { messageRequest });

            if (rpcMethod.ReturnType != typeof(void))
            {
                var genericReplyMethod = _replyMethod.MakeGenericMethod(new Type[] { rpcMethod.ReturnType });

                var responseEnvelope = typeof(ActorMessageEnvelope<>);
                var responseType = responseEnvelope.MakeGenericType(rpcMethod.ReturnType);
                var responseInstance = Activator.CreateInstance(responseType);
                responseType.GetProperty("CorrelationID").SetValue(responseInstance, messageID);
                responseType.GetProperty("CorrelationTime").SetValue(responseInstance, messageTime);
                responseType.GetProperty("Message").SetValue(responseInstance, messageResponse);

                var replyArguments = new List<object>();
                replyArguments.Add(channelIdentifier);
                replyArguments.Add(responseInstance);

                genericReplyMethod.Invoke(invocation.Proxy, replyArguments.ToArray());
            }
        }
    }
}
