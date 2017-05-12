using System.Collections.Generic;
using Logrila.Logging;
using Redola.ActorModel;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    public class OrderService : RpcService
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

        private void OnPlaceOrderRequest(ActorIdentity remoteActor, ActorMessageEnvelope<PlaceOrderRequest> request)
        {
            var response = new ActorMessageEnvelope<PlaceOrderResponse>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = new PlaceOrderResponse()
                {
                    Contract = request.Message.Contract,
                    Order = request.Message.Contract,
                    ErrorCode = PlaceOrderErrorCode.OrderPlaced,
                },
            };

            _log.DebugFormat("OnPlaceOrderRequest, place order, MessageID[{0}], CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
            this.Actor.BeginSend(remoteActor, response);
        }

        public void NotifyOrderChanged(ActorMessageEnvelope<OrderStatusChangedNotification> notification)
        {
            this.Actor.BeginSend("client", notification);
        }
    }
}
