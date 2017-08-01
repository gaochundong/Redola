using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient.ConsulIntegration
{
    internal class OrderEventService : IOrderEventService
    {
        private ILog _log = Logger.Get<OrderEventService>();

        public OrderDeliveredConfirmation OrderDelivered(OrderDeliveredNotification request)
        {
            _log.DebugFormat("OrderDelivered, OrderID[{0}].", request.OrderID);
            return new OrderDeliveredConfirmation()
            {
                OrderID = request.OrderID,
            };
        }
    }
}
