using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpServer
{
    public class CalcClient : RpcService
    {
        private ILog _log = Logger.Get<CalcClient>();

        public CalcClient(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(AddRequest), typeof(AddResponse)));

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
