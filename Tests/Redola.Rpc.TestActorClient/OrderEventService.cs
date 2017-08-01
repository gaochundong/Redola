using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestActorClient
{
    internal class OrderEventService : RpcHandler, IOrderEventService
    {
        private ILog _log = Logger.Get<OrderEventService>();

        public OrderEventService(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new ReceiveMessageContract(typeof(OrderDeliveredNotification)));

            return messages;
        }

        private void OnOrderDeliveredNotification(ActorSender sender, ActorMessageEnvelope<OrderDeliveredNotification> request)
        {
            _log.DebugFormat("OnOrderDeliveredNotification, MessageID[{0}], CorrelationID[{1}].",
                request.MessageID, request.CorrelationID);

            var response = new ActorMessageEnvelope<OrderDeliveredConfirmation>()
            {
                CorrelationID = request.MessageID,
                CorrelationTime = request.MessageTime,
                Message = OrderDelivered(request.Message),
            };

            this.BeginReply(sender.ChannelIdentifier, response);
        }

        public OrderDeliveredConfirmation OrderDelivered(OrderDeliveredNotification request)
        {
            _log.DebugFormat("OrderDelivered, OrderID[{0}], OrderStatus[{1}].",
                request.OrderID, request.OrderStatus);
            return new OrderDeliveredConfirmation() { OrderID = request.OrderID };
        }
    }
}
