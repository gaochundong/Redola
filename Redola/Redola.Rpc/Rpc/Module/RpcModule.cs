using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public class RpcModule : RpcHandler
    {
        private IServiceCatalogProvider _serviceCatalogProvider;
        private MethodRouteResolver _methodRouteResolver;

        public RpcModule(RpcActor localActor, IServiceCatalogProvider catalog)
            : base(localActor)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");
            _serviceCatalogProvider = catalog;

            Initialize();
        }

        public RpcModule(RpcActor localActor, IRateLimiter rateLimiter, IServiceCatalogProvider catalog)
            : base(localActor, rateLimiter)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");
            _serviceCatalogProvider = catalog;

            Initialize();
        }

        private void Initialize()
        {
            var services = _serviceCatalogProvider.GetServices();
            var locatorExtractor = new MethodLocatorExtractor();
            var routeBuilder = new MethodRouteBuilder(locatorExtractor);
            var routeCache = routeBuilder.BuildCache(services);
            _methodRouteResolver = new MethodRouteResolver(routeCache);
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
    }
}
