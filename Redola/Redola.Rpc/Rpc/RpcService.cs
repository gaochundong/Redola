using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public abstract class RpcService : BlockingActorMessageHandlerBase
    {
        public RpcService(RpcActor localActor)
            : base(localActor)
        {
        }

        protected sealed override void RegisterAdmissibleMessages(IDictionary<string, MessageHandleStrategy> admissibleMessages)
        {
            base.RegisterAdmissibleMessages(admissibleMessages);

            var rpcMessages = RegisterRpcMessages();
            if (rpcMessages != null)
            {
                foreach (var pair in rpcMessages)
                {
                    admissibleMessages.Add(pair.Item2.Name, new MessageHandleStrategy(pair.Item2));
                }
            }
        }

        protected abstract IEnumerable<Tuple<Type, Type>> RegisterRpcMessages();
    }
}
