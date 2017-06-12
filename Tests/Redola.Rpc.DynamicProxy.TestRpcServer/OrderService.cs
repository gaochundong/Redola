using Redola.Rpc.TestContracts;

namespace Redola.Rpc.DynamicProxy.TestRpcServer
{
    internal class OrderService : IOrderService
    {
        public PlaceOrderResponse PlaceOrder(PlaceOrderRequest request)
        {
            return new PlaceOrderResponse()
            {
                Contract = request.Contract,
                Order = request.Contract,
                ErrorCode = PlaceOrderErrorCode.OrderPlaced,
            };
        }
    }
}
