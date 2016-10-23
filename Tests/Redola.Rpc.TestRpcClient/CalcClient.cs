using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    public class CalcClient : RpcService
    {
        private ILog _log = Logger.Get<CalcClient>();

        public CalcClient(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageRegistration> RegisterRpcMessages()
        {
            var messages = new List<RpcMessageRegistration>();

            messages.Add(new RpcMessageRegistration(typeof(AddResponse)) { IsRequestResponseModel = true });

            return messages;
        }

        public int Add(int x, int y)
        {
            var request = new ActorMessageEnvelope<AddRequest>()
            {
                Message = new AddRequest() { X = x, Y = y },
            };
            return this.Actor.Send<AddRequest, AddResponse>("server", request).Message.Result;
        }
    }
}
