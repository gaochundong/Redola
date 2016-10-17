using System.Collections.Generic;
using Logrila.Logging;

namespace Redola.Rpc
{
    public class RpcMessageHandler : BlockingActorMessageHandlerBase
    {
        private ILog _log = Logger.Get<RpcMessageHandler>();

        public RpcMessageHandler(BlockingRouteActor localActor)
            : base(localActor)
        {
        }

        protected override void RegisterAdmissibleMessages(IDictionary<string, MessageHandleStrategy> admissibleMessages)
        {
            base.RegisterAdmissibleMessages(admissibleMessages);
        }
    }
}
