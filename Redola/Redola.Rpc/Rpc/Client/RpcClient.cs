using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcClient : RpcHandler
    {
        private IActorDirectory _directory;
        private IServiceProxyGenerator _proxyGenerator;
        private RpcMethodFixture _fixture;

        public RpcClient(RpcActor localActor, IActorDirectory directory, IServiceProxyGenerator proxyGenerator)
            : this(localActor, directory, proxyGenerator,
                  new RpcMethodFixture(
                    new MethodLocatorExtractor(),
                    new MethodArgumentEncoder(RpcActor.DefaultObjectEncoder),
                    new MethodArgumentDecoder(RpcActor.DefaultObjectDecoder)))
        {
        }

        public RpcClient(RpcActor localActor, IActorDirectory directory, IServiceProxyGenerator proxyGenerator, RpcMethodFixture fixture)
            : base(localActor)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (proxyGenerator == null)
                throw new ArgumentNullException("proxyGenerator");
            if (fixture == null)
                throw new ArgumentNullException("fixture");

            _directory = directory;
            _proxyGenerator = proxyGenerator;
            _fixture = fixture;

            Initialize();
        }

        public RpcClient(RpcActor localActor, IRateLimiter rateLimiter, IActorDirectory directory, IServiceProxyGenerator proxyGenerator, RpcMethodFixture fixture)
            : base(localActor, rateLimiter)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (proxyGenerator == null)
                throw new ArgumentNullException("proxyGenerator");
            if (fixture == null)
                throw new ArgumentNullException("fixture");

            _directory = directory;
            _proxyGenerator = proxyGenerator;
            _fixture = fixture;

            Initialize();
        }

        private void Initialize()
        {
            this.Actor.RegisterRpcHandler(this);
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(InvokeMethodRequest), typeof(InvokeMethodResponse)));

            return messages;
        }

        public T Resolve<T>()
        {
            return _proxyGenerator.CreateServiceProxy<T>(this, _fixture);
        }

        public T Resolve<T>(IServiceLoadBalancingStrategy strategy)
        {
            return _proxyGenerator.CreateServiceProxy<T>(this, _fixture, strategy);
        }

        public void Bootup()
        {
            this.Actor.Bootup(_directory);
        }

        public void Shutdown()
        {
            this.Actor.Shutdown();
        }
    }
}
