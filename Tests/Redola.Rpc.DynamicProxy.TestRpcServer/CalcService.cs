using Redola.Rpc.TestContracts;

namespace Redola.Rpc.DynamicProxy.TestRpcServer
{
    internal class CalcService : ICalcService
    {
        public AddResponse Add(AddRequest request)
        {
            return new AddResponse() { Result = request.X + request.Y };
        }
    }
}
