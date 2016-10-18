using System;
using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpServer
{
    public class HelloClient : RpcService
    {
        private ILog _log = Logger.Get<HelloClient>();

        public HelloClient(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageRegistration> RegisterRpcMessages()
        {
            var messages = new List<RpcMessageRegistration>();

            messages.Add(new RpcMessageRegistration(typeof(HelloResponse)) { IsRequestResponseModel = true });

            return messages;
        }

        public ActorMessageEnvelope<HelloResponse> SayHello()
        {
            var request = new ActorMessageEnvelope<HelloRequest>()
            {
                Message = new HelloRequest() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
            };

            return this.Actor.Send<HelloRequest, HelloResponse>("server", request);
        }
    }
}
