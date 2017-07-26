using System;
using System.Collections.Generic;
using System.Linq;

namespace Redola.Rpc
{
    public class RpcServiceModule : RpcService
    {
        private IList<Tuple<Type, object>> _services = new List<Tuple<Type, object>>();
        private MethodRouteResolver _methodRouteResolver;

        public RpcServiceModule(RpcActor localActor)
            : base(localActor)
        {
        }

        public RpcServiceModule(RpcActor localActor, IRateLimiter rateLimiter)
            : base(localActor, rateLimiter)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new ReceiveMessageContract(typeof(InvokeMethodMessage)));
            messages.Add(new ReceiveMessageContract(typeof(InvokeMethodRequest)));

            return messages;
        }

        private void OnInvokeMethodMessage(ActorSender sender, ActorMessageEnvelope<InvokeMethodMessage> request)
        {
            InvokeMethod(request.Message);
        }

        private void OnInvokeMethodRequest(ActorSender sender, ActorMessageEnvelope<InvokeMethodRequest> request)
        {
            var response = new ActorMessageEnvelope<InvokeMethodResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = InvokeMethod(request.Message),
            };

            this.BeginReply(sender.ChannelIdentifier, response);
        }

        private void InvokeMethod(InvokeMethodMessage message)
        {
            var methodRoute = _methodRouteResolver.Resolve(message.MethodLocator);
            if (methodRoute == null)
                throw new InvalidOperationException(string.Format(
                    "Cannot resolve method route [{0}].", message.MethodLocator));

            methodRoute.Invoke(message.MethodArguments);
        }

        private InvokeMethodResponse InvokeMethod(InvokeMethodRequest request)
        {
            var methodRoute = _methodRouteResolver.Resolve(request.MethodLocator);
            if (methodRoute == null)
                throw new InvalidOperationException(string.Format(
                    "Cannot resolve method route [{0}].", request.MethodLocator));

            var returnValue = methodRoute.InvokeReturn(request.MethodArguments);

            var response = new InvokeMethodResponse()
            {
                MethodLocator = request.MethodLocator,
                MethodReturnValue = returnValue,
            };
            return response;
        }

        public void RegisterService<T>(T service)
        {
            RegisterService(typeof(T), service);
        }

        public void RegisterService(Type declaringType, object service)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (service == null)
                throw new ArgumentNullException("service");

            lock (_services)
            {
                if (_services.Any(s => s.Item1.Equals(declaringType)))
                    throw new ArgumentException(string.Format("Type [{0}] has already been registered.", declaringType), "service");

                _services.Add(new Tuple<Type, object>(declaringType, service));

                var locatorExtractor = new MethodLocatorExtractor();
                var routeBuilder = new MethodRouteBuilder(locatorExtractor);
                var routeCache = routeBuilder.BuildCache(_services);
                _methodRouteResolver = new MethodRouteResolver(routeCache);
            }
        }
    }
}
