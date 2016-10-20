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

        protected override IEnumerable<RpcMessageRegistration> RegisterRpcMessages()
        {
            var messages = new List<RpcMessageRegistration>();

            messages.Add(new RpcMessageRegistration(typeof(PlaceOrderResponse)) { IsRequestResponseModel = true });

            messages.Add(new RpcMessageRegistration(typeof(OrderStatusChangedNotification)) { IsRequestResponseModel = false });

            return messages;
        }

        public ActorMessageEnvelope<PlaceOrderResponse> PlaceOrder(ActorMessageEnvelope<PlaceOrderRequest> request)
        {
            return this.Actor.Send<PlaceOrderRequest, PlaceOrderResponse>("server", request);
        }

        private void OnOrderStatusChangedNotification(ActorDescription remoteActor, ActorMessageEnvelope<OrderStatusChangedNotification> message)
        {
            _log.DebugFormat("OnOrderStatusChangedNotification, order changed, MessageID[{0}], CorrelationID[{1}].",
                message.MessageID, message.CorrelationID);
        }
    }
}
