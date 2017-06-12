using System;
using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    internal class HelloService : RpcService, IHelloService
    {
        private ILog _log = Logger.Get<HelloService>();

        public HelloService(RpcActor localActor)
            : base(localActor)
        {
        }

        public HelloService(RpcActor localActor, IRateLimiter rateLimiter)
            : base(localActor, rateLimiter)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new ReceiveMessageContract(typeof(HelloRequest)));
            messages.Add(new ReceiveMessageContract(typeof(Hello10000Request)));

            return messages;
        }

        private void OnHelloRequest(ActorSender sender, ActorMessageEnvelope<HelloRequest> request)
        {
            var response = new ActorMessageEnvelope<HelloResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = Hello(request.Message),
            };

            _log.DebugFormat("OnHelloRequest, say hello, MessageID[{0}], CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
            this.BeginReply(sender.ChannelIdentifier, response);
        }

        private void OnHello10000Request(ActorSender sender, ActorMessageEnvelope<Hello10000Request> request)
        {
            var response = new ActorMessageEnvelope<Hello10000Response>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = Hello10000(request.Message),
            };

            this.BeginReply(sender.ChannelIdentifier, response);
        }

        public HelloResponse Hello(HelloRequest request)
        {
            return new HelloResponse() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") };
        }

        public Hello10000Response Hello10000(Hello10000Request request)
        {
            return new Hello10000Response() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") };
        }
    }
}
