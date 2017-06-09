using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    internal class OrderClient : RpcService, IOrderService
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

            messages.Add(new ReceiveMessageContract(typeof(OrderStatusChangedNotification)));

            return messages;
        }

        public PlaceOrderResponse PlaceOrder(PlaceOrderRequest request)
        {
            var envelope = new ActorMessageEnvelope<PlaceOrderRequest>()
            {
                Message = request,
            };
            return this.Actor.Send<PlaceOrderRequest, PlaceOrderResponse>("server", envelope).Message;
        }

        private void OnOrderStatusChangedNotification(ActorSender sender, ActorMessageEnvelope<OrderStatusChangedNotification> message)
        {
            _log.DebugFormat("OnOrderStatusChangedNotification, order changed, MessageID[{0}], CorrelationID[{1}].",
                message.MessageID, message.CorrelationID);
        }
    }
}
