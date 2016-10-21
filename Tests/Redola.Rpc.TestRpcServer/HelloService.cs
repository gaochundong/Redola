using System;
using System.Collections.Generic;
using Logrila.Logging;
using Redola.ActorModel;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    public class HelloService : RpcService
    {
        private ILog _log = Logger.Get<HelloService>();

        public HelloService(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageRegistration> RegisterRpcMessages()
        {
            var messages = new List<RpcMessageRegistration>();

            messages.Add(new RpcMessageRegistration(typeof(HelloRequest)));
            messages.Add(new RpcMessageRegistration(typeof(Hello10000Request)));

            return messages;
        }

        private void OnHelloRequest(ActorIdentity remoteActor, ActorMessageEnvelope<HelloRequest> request)
        {
            var response = new ActorMessageEnvelope<HelloResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = new HelloResponse() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
            };

            _log.DebugFormat("OnHelloRequest, say hello, MessageID[{0}], CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
            this.Actor.BeginSend(remoteActor, response);
        }

        private void OnHello10000Request(ActorIdentity remoteActor, ActorMessageEnvelope<Hello10000Request> request)
        {
            var response = new ActorMessageEnvelope<Hello10000Response>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = new Hello10000Response() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
            };

            this.Actor.BeginSend(remoteActor, response);
        }
    }
}
