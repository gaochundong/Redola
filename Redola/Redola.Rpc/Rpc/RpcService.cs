using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public abstract class RpcService : BlockingActorMessageHandlerBase
    {
        private RpcActor _localActor;
        private IRateLimiter _rateLimiter = null;

        public RpcService(RpcActor localActor)
            : base(localActor.Actor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            _localActor = localActor;
        }

        public RpcService(RpcActor localActor, IRateLimiter rateLimiter)
            : this(localActor)
        {
            if (rateLimiter == null)
                throw new ArgumentNullException("rateLimiter");
            _rateLimiter = rateLimiter;
        }

        public new RpcActor Actor { get { return _localActor; } }

        public bool IsRateLimited { get { return _rateLimiter != null; } }

        protected sealed override void RegisterAdmissibleMessages(IDictionary<string, MessageHandleStrategy> admissibleMessages)
        {
            base.RegisterAdmissibleMessages(admissibleMessages);

            var registrations = RegisterRpcMessageContracts();
            if (registrations != null)
            {
                foreach (var registration in registrations)
                {
                    admissibleMessages.Add(registration.MessageType.Name, registration.ToStrategy());
                }
            }
        }

        protected abstract IEnumerable<RpcMessageContract> RegisterRpcMessageContracts();

        protected override void DoHandleMessage(ActorSender sender, ActorMessageEnvelope envelope)
        {
            if (IsRateLimited)
            {
                _rateLimiter.Wait();
                try
                {
                    base.DoHandleMessage(sender, envelope);
                }
                finally
                {
                    _rateLimiter.Release();
                }
            }
            else
            {
                base.DoHandleMessage(sender, envelope);
            }
        }
    }
}
