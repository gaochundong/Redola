using System.Collections.Generic;
using Logrila.Logging;
using Redola.ActorModel;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    public class CalcService : RpcService
    {
        private ILog _log = Logger.Get<CalcService>();

        public CalcService(RpcActor localActor)
            : base(localActor)
        {
        }

        public CalcService(RpcActor localActor, IRateLimiter rateLimiter)
            : base(localActor, rateLimiter)
        {
        }

        protected override IEnumerable<RpcMessageRegistration> RegisterRpcMessages()
        {
            var messages = new List<RpcMessageRegistration>();

            messages.Add(new RpcMessageRegistration(typeof(AddRequest)));

            return messages;
        }

        private void OnAddRequest(ActorIdentity remoteActor, ActorMessageEnvelope<AddRequest> request)
        {
            var response = new ActorMessageEnvelope<AddResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = new AddResponse() { Result = request.Message.X + request.Message.Y },
            };

            this.Actor.BeginSend(remoteActor, response);
        }
    }
}
