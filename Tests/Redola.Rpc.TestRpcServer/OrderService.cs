using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    internal class OrderService : RpcService, IOrderService
    {
        private ILog _log = Logger.Get<OrderService>();

        public OrderService(RpcActor localActor)
            : base(localActor)
        {
        }

        public OrderService(RpcActor localActor, IRateLimiter rateLimiter)
            : base(localActor, rateLimiter)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new ReceiveMessageContract(typeof(PlaceOrderRequest)));

            return messages;
        }

        private void OnPlaceOrderRequest(ActorSender sender, ActorMessageEnvelope<PlaceOrderRequest> request)
        {
            var response = new ActorMessageEnvelope<PlaceOrderResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = PlaceOrder(request.Message),
            };

            _log.DebugFormat("OnPlaceOrderRequest, place order, MessageID[{0}], CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
            this.BeginReply(sender.ChannelIdentifier, response);
        }

        public PlaceOrderResponse PlaceOrder(PlaceOrderRequest request)
        {
            return new PlaceOrderResponse()
            {
                Contract = request.Contract,
                Order = request.Contract,
                ErrorCode = PlaceOrderErrorCode.OrderPlaced,
            };
        }

        public void NotifyOrderChanged(ActorMessageEnvelope<OrderStatusChangedNotification> notification)
        {
            this.BeginSend("client", notification);
        }
    }
}
