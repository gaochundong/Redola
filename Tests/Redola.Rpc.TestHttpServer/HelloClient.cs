using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpServer
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
            return this.Actor.Send<HelloRequest, HelloResponse>("server", request);
        }

        public Hello10000Response Hello10000(Hello10000Request request)
        {
            return this.Actor.Send<Hello10000Request, Hello10000Response>("server", request);
        }
    }
}
