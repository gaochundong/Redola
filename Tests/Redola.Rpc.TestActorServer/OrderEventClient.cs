using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestActorServer
{
    internal class OrderEventClient : RpcHandler, IOrderEventService
    {
        private ILog _log = Logger.Get<OrderEventClient>();

        public OrderEventClient(RpcActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(OrderDeliveredNotification), typeof(OrderDeliveredConfirmation)));

            return messages;
        }

        public OrderDeliveredConfirmation OrderDelivered(OrderDeliveredNotification request)
        {
            return this.Send<OrderDeliveredNotification, OrderDeliveredConfirmation>("client", request);
        }
    }
}
