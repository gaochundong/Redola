using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    internal class HelloClient : RpcService, IHelloService
    {
        private ILog _log = Logger.Get<HelloClient>();

        public HelloClient(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(HelloRequest), typeof(HelloResponse)));
            messages.Add(new RequestResponseMessageContract(typeof(Hello10000Request), typeof(Hello10000Response)));

            return messages;
        }

        public HelloResponse Hello(HelloRequest request)
        {
            var envelope = new ActorMessageEnvelope<HelloRequest>()
            {
                Message = request,
            };
            return this.Actor.Send<HelloRequest, HelloResponse>("server", envelope).Message;
        }

        public Hello10000Response Hello10000(Hello10000Request request)
        {
            var envelope = new ActorMessageEnvelope<Hello10000Request>()
            {
                Message = request,
            };
            return this.Actor.Send<Hello10000Request, Hello10000Response>("server", envelope).Message;
        }
    }
}
