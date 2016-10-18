using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public abstract class RpcService : BlockingActorMessageHandlerBase
    {
        private RpcActor _localActor;

        public RpcService(RpcActor localActor)
            : base(localActor.Actor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            _localActor = localActor;
        }

        public new RpcActor Actor { get { return _localActor; } }

        protected sealed override void RegisterAdmissibleMessages(IDictionary<string, MessageHandleStrategy> admissibleMessages)
        {
            base.RegisterAdmissibleMessages(admissibleMessages);

            var registrations = RegisterRpcMessages();
            if (registrations != null)
            {
                foreach (var registration in registrations)
                {
                    admissibleMessages.Add(registration.MessageType.Name, registration.ToStrategy());
                }
            }
        }

        protected abstract IEnumerable<RpcMessageRegistration> RegisterRpcMessages();
    }
}
