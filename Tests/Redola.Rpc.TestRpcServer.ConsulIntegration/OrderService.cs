using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer.ConsulIntegration
{
    internal class OrderService : IOrderService
    {
        private ILog _log = Logger.Get<OrderService>();

        public PlaceOrderResponse PlaceOrder(PlaceOrderRequest request)
        {
            _log.DebugFormat("PlaceOrder, OrderID={0}", request.Contract.OrderID);
            return new PlaceOrderResponse()
            {
                Contract = request.Contract,
                Order = request.Contract,
                ErrorCode = PlaceOrderErrorCode.OrderPlaced,
            };
        }
    }
}
