using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.DynamicProxy.CastleIntegration.TestRpcServer
{
    internal class CalcService : ICalcService
    {
        private ILog _log = Logger.Get<CalcService>();

        public AddResponse Add(AddRequest request)
        {
            var response = new AddResponse() { Result = request.X + request.Y };
            _log.DebugFormat("Add, X={0}, Y={1}, Result={2}", request.X, request.Y, response.Result);
            return response;
        }
    }
}
