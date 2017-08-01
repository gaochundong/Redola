using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcClient : RpcHandler
    {
        private IActorDirectory _actorDirectory;
        private IServiceProxyGenerator _proxyGenerator;
        private RpcMethodFixture _methodFixture;
        private readonly object _bootupLock = new object();

        public RpcClient(RpcActor localActor, IActorDirectory actorDirectory, IServiceProxyGenerator proxyGenerator)
            : this(localActor, actorDirectory, proxyGenerator,
                  new RpcMethodFixture(
                    new MethodLocatorExtractor(),
                    new MethodArgumentEncoder(RpcActor.DefaultObjectEncoder),
                    new MethodArgumentDecoder(RpcActor.DefaultObjectDecoder)))
        {
        }

        public RpcClient(RpcActor localActor, IActorDirectory actorDirectory, IServiceProxyGenerator proxyGenerator, RpcMethodFixture methodFixture)
            : base(localActor)
        {
            if (actorDirectory == null)
                throw new ArgumentNullException("actorDirectory");
            if (proxyGenerator == null)
                throw new ArgumentNullException("proxyGenerator");
            if (methodFixture == null)
                throw new ArgumentNullException("methodFixture");

            _actorDirectory = actorDirectory;
            _proxyGenerator = proxyGenerator;
            _methodFixture = methodFixture;
        }

        public RpcClient(RpcActor localActor, IRateLimiter rateLimiter, IActorDirectory actorDirectory, IServiceProxyGenerator proxyGenerator, RpcMethodFixture methodFixture)
            : base(localActor, rateLimiter)
        {
            if (actorDirectory == null)
                throw new ArgumentNullException("actorDirectory");
            if (proxyGenerator == null)
                throw new ArgumentNullException("proxyGenerator");
            if (methodFixture == null)
                throw new ArgumentNullException("methodFixture");

            _actorDirectory = actorDirectory;
            _proxyGenerator = proxyGenerator;
            _methodFixture = methodFixture;
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(InvokeMethodRequest), typeof(InvokeMethodResponse)));

            return messages;
        }

        public T Resolve<T>()
        {
            return _proxyGenerator.CreateServiceProxy<T>(this, _methodFixture);
        }

        public T Resolve<T>(IServiceLoadBalancingStrategy strategy)
        {
            return _proxyGenerator.CreateServiceProxy<T>(this, _methodFixture, strategy);
        }

        public void Bootup()
        {
            lock (_bootupLock)
            {
                if (this.Actor.Active)
                    throw new InvalidOperationException("The actor has already been bootup.");

                this.Actor.RegisterRpcHandler(this);

                this.Actor.Bootup(_actorDirectory);
            }
        }

        public void Shutdown()
        {
            lock (_bootupLock)
            {
                if (this.Actor.Active)
                {
                    this.Actor.Shutdown();
                }
            }
        }
    }
}
