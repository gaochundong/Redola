using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public class RpcServer : RpcHandler
    {
        private IServiceCatalogProvider _catalog;
        private RpcMethodFixture _fixture;
        private MethodRouteResolver _resolver;

        public RpcServer(RpcActor localActor, IServiceCatalogProvider catalog, RpcMethodFixture fixture)
            : base(localActor)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");
            if (fixture == null)
                throw new ArgumentNullException("fixture");

            _catalog = catalog;
            _fixture = fixture;

            Initialize();
        }

        public RpcServer(RpcActor localActor, IRateLimiter rateLimiter, IServiceCatalogProvider catalog, RpcMethodFixture fixture)
            : base(localActor, rateLimiter)
        {
            if (catalog == null)
                throw new ArgumentNullException("catalog");
            if (fixture == null)
                throw new ArgumentNullException("fixture");

            _catalog = catalog;
            _fixture = fixture;

            Initialize();
        }

        private void Initialize()
        {
            var services = _catalog.GetServices();
            var routeBuilder = new MethodRouteBuilder(_fixture.Extractor);
            var routeCache = routeBuilder.BuildCache(services);
            _resolver = new MethodRouteResolver(routeCache);

            this.Actor.RegisterRpcHandler(this);
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
            request.Message.Deserialize(_fixture.ArgumentDecoder);
            InvokeMethod(request.Message);
        }

        private void OnInvokeMethodRequest(ActorSender sender, ActorMessageEnvelope<InvokeMethodRequest> request)
        {
            request.Message.Deserialize(_fixture.ArgumentDecoder);
            var message = InvokeMethod(request.Message);
            message.Serialize(_fixture.ArgumentEncoder);

            var response = new ActorMessageEnvelope<InvokeMethodResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = message,
            };

            this.BeginReply(sender.ChannelIdentifier, response);
        }

        private void InvokeMethod(InvokeMethodMessage message)
        {
            var methodRoute = _resolver.Resolve(message.MethodLocator);
            if (methodRoute == null)
                throw new InvalidOperationException(string.Format(
                    "Cannot resolve method route [{0}].", message.MethodLocator));

            methodRoute.Invoke(message.MethodArguments);
        }

        private InvokeMethodResponse InvokeMethod(InvokeMethodRequest request)
        {
            var methodRoute = _resolver.Resolve(request.MethodLocator);
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
