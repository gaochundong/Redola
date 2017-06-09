using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    internal class CalcClient : RpcService, ICalcService
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
            return Add(new AddRequest() { X = x, Y = y }).Result;
        }

        public AddResponse Add(AddRequest request)
        {
            var envelope = new ActorMessageEnvelope<AddRequest>()
            {
                Message = request,
            };
            return this.Actor.Send<AddRequest, AddResponse>("server", envelope).Message;
        }
    }
}
