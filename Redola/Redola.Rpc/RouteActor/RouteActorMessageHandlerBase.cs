using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logrila.Logging;

namespace Redola.Rpc
{
    public abstract class RouteActorMessageHandlerBase : IRouteActorMessageHandler
    {
        private ILog _log = Logger.Get<RouteActorMessageHandlerBase>();
        private RouteActor _localActor;
        private Dictionary<string, MessageHandleStrategy> _admissibleMessages = new Dictionary<string, MessageHandleStrategy>();

        public RouteActorMessageHandlerBase(RouteActor localActor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            _localActor = localActor;

            RegisterAdmissibleMessages(_admissibleMessages);
        }

        public RouteActor Actor { get { return _localActor; } }

        protected virtual void RegisterAdmissibleMessages(IDictionary<string, MessageHandleStrategy> admissibleMessages)
        {
        }

        protected virtual Type GetAdmissibleMessageType(string messageType)
        {
            return _admissibleMessages[messageType].MessageType;
        }

        protected virtual MessageHandleStrategy GetAdmissibleMessageHandleStrategy(string messageType)
        {
            return _admissibleMessages[messageType];
        }

        public virtual bool CanHandleMessage(ActorMessageEnvelope envelope)
        {
            return _admissibleMessages.ContainsKey(envelope.MessageType);
        }

        public virtual void HandleMessage(ActorSender sender, ActorMessageEnvelope envelope)
        {
            if (GetAdmissibleMessageHandleStrategy(envelope.MessageType).IsAsyncPattern)
            {
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        DoHandleMessage(sender, envelope);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                },
                TaskCreationOptions.PreferFairness);
            }
            else
            {
                DoHandleMessage(sender, envelope);
            }
        }

        protected virtual void DoHandleMessage(ActorSender sender, ActorMessageEnvelope envelope)
        {
            envelope.HandledBy(this, GetAdmissibleMessageType(envelope.MessageType), this.Actor.Decoder, sender);
        }
    }
}
