using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Logrila.Logging;

namespace Redola.ActorModel
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

        public Type GetAdmissibleMessageType(string messageType)
        {
            return _admissibleMessages[messageType].MessageType;
        }

        public bool IsMessageHandledAsyncWay(string messageType)
        {
            return _admissibleMessages[messageType].AsyncWay;
        }

        public bool CanHandleMessage(MessageEnvelope envelope)
        {
            return _admissibleMessages.ContainsKey(envelope.MessageType);
        }

        public virtual void HandleMessage(ActorDescription remoteActor, MessageEnvelope envelope)
        {
            if (!IsMessageHandledAsyncWay(envelope.MessageType))
            {
                DoHandleMessage(remoteActor, envelope);
            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        DoHandleMessage(remoteActor, envelope);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex.Message, ex);
                    }
                });
            }
        }

        protected virtual void DoHandleMessage(ActorDescription remoteActor, MessageEnvelope envelope)
        {
            envelope.HandledBy(this, GetAdmissibleMessageType(envelope.MessageType), this.Actor.Decoder, remoteActor);
        }
    }
}
