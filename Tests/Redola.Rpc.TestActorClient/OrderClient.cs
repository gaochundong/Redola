using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestActorClient
{
    internal class OrderClient : RpcHandler, IOrderService
    {
        private ILog _log = Logger.Get<OrderClient>();

        public OrderClient(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(PlaceOrderRequest), typeof(PlaceOrderResponse)));

            return messages;
        }

        public PlaceOrderResponse PlaceOrder(PlaceOrderRequest request)
        {
            return this.Send<PlaceOrderRequest, PlaceOrderResponse>("server", request);
        }
    }
}
