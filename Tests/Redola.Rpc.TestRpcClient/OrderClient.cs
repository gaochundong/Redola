using System.Collections.Generic;
using Logrila.Logging;
using Redola.ActorModel;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    public class OrderClient : RpcService
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

        public ActorMessageEnvelope<PlaceOrderResponse> PlaceOrder(ActorMessageEnvelope<PlaceOrderRequest> request)
        {
            return this.Actor.Send<PlaceOrderRequest, PlaceOrderResponse>("server", request);
        }

        private void OnOrderStatusChangedNotification(ActorIdentity remoteActor, ActorMessageEnvelope<OrderStatusChangedNotification> message)
        {
            _log.DebugFormat("OnOrderStatusChangedNotification, order changed, MessageID[{0}], CorrelationID[{1}].",
                message.MessageID, message.CorrelationID);
        }
    }
}
