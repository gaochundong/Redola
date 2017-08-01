using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcServer : RpcHandler
    {
        private IActorDirectory _actorDirectory;
        private IServiceCatalogProvider _serviceCatalog;
        private IServiceDirectory _serviceDirectory;
        private RpcMethodFixture _methodFixture;
        private MethodRouteResolver _methodResolver;
        private readonly object _bootupLock = new object();

        public RpcServer(RpcActor localActor, IActorDirectory actorDirectory, IServiceCatalogProvider serviceCatalog, IServiceDirectory serviceDirectory)
            : this(localActor, actorDirectory, serviceCatalog, serviceDirectory,
                  new RpcMethodFixture(
                    new MethodLocatorExtractor(),
                    new MethodArgumentEncoder(RpcActor.DefaultObjectEncoder),
                    new MethodArgumentDecoder(RpcActor.DefaultObjectDecoder)))
        {
        }

        public RpcServer(RpcActor localActor, IActorDirectory actorDirectory, IServiceCatalogProvider serviceCatalog, IServiceDirectory serviceDirectory, RpcMethodFixture methodFixture)
            : base(localActor)
        {
            if (actorDirectory == null)
                throw new ArgumentNullException("actorDirectory");
            if (serviceCatalog == null)
                throw new ArgumentNullException("serviceCatalog");
            if (serviceDirectory == null)
                throw new ArgumentNullException("serviceDirectory");
            if (methodFixture == null)
                throw new ArgumentNullException("methodFixture");

            _actorDirectory = actorDirectory;
            _serviceCatalog = serviceCatalog;
            _serviceDirectory = serviceDirectory;
            _methodFixture = methodFixture;
        }

        public RpcServer(RpcActor localActor, IRateLimiter rateLimiter, IActorDirectory actorDirectory, IServiceCatalogProvider serviceCatalog, IServiceDirectory serviceDirectory, RpcMethodFixture methodFixture)
            : base(localActor, rateLimiter)
        {
            if (actorDirectory == null)
                throw new ArgumentNullException("actorDirectory");
            if (serviceCatalog == null)
                throw new ArgumentNullException("serviceCatalog");
            if (serviceDirectory == null)
                throw new ArgumentNullException("serviceDirectory");
            if (methodFixture == null)
                throw new ArgumentNullException("methodFixture");

            _actorDirectory = actorDirectory;
            _serviceCatalog = serviceCatalog;
            _serviceDirectory = serviceDirectory;
            _methodFixture = methodFixture;
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
            request.Message.Deserialize(_methodFixture.ArgumentDecoder);
            InvokeMethod(request.Message);
        }

        private void OnInvokeMethodRequest(ActorSender sender, ActorMessageEnvelope<InvokeMethodRequest> request)
        {
            request.Message.Deserialize(_methodFixture.ArgumentDecoder);
            var message = InvokeMethod(request.Message);
            message.Serialize(_methodFixture.ArgumentEncoder);

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
            var methodRoute = _methodResolver.Resolve(message.MethodLocator);
            if (methodRoute == null)
                throw new InvalidOperationException(string.Format(
                    "Cannot resolve method route [{0}].", message.MethodLocator));

            methodRoute.Invoke(message.MethodArguments);
        }

        private InvokeMethodResponse InvokeMethod(InvokeMethodRequest request)
        {
            var methodRoute = _methodResolver.Resolve(request.MethodLocator);
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

        public void Bootup()
        {
            lock (_bootupLock)
            {
                if (this.Actor.Active)
                    throw new InvalidOperationException("The actor has already been bootup.");

                var services = _serviceCatalog.GetServices();
                var routeBuilder = new MethodRouteBuilder(_methodFixture.Extractor);
                var routeCache = routeBuilder.BuildCache(services);
                _methodResolver = new MethodRouteResolver(routeCache);

                this.Actor.RegisterRpcHandler(this);

                this.Actor.Bootup(_actorDirectory);

                foreach (var service in services)
                {
                    _serviceDirectory.RegisterService(this.Actor.Identity, service.DeclaringType);
                }
            }
        }

        public void Shutdown()
        {
            lock (_bootupLock)
            {
                if (this.Actor.Active)
                {
                    var services = _serviceCatalog.GetServices();

                    foreach (var service in services)
                    {
                        _serviceDirectory.DeregisterService(this.Actor.Identity, service.DeclaringType);
                    }

                    this.Actor.DeregisterRpcHandler(this);

                    this.Actor.Shutdown();
                }
            }
        }
    }
}
