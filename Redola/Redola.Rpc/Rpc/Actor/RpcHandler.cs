using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public abstract class RpcHandler : BlockingActorMessageHandlerBase
    {
        private RpcActor _localActor;
        private IRateLimiter _rateLimiter = null;

        public RpcHandler(RpcActor localActor)
            : base(localActor.Actor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            _localActor = localActor;
        }

        public RpcHandler(RpcActor localActor, IRateLimiter rateLimiter)
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

        #region Blocking Envelope

        public ActorMessageEnvelope<P> Send<R, P>(ActorIdentity remoteActor, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActor, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(ActorIdentity remoteActor, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return this.Actor.Send<R, P>(remoteActor, request, timeout);
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActorType, remoteActorName, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return this.Actor.Send<R, P>(remoteActorType, remoteActorName, request, timeout);
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return this.Actor.Send<R, P>(remoteActorType, request, timeout);
        }

        #endregion

        #region Blocking Message

        public P Send<R, P>(ActorIdentity remoteActor, R request)
        {
            return Send<R, P>(remoteActor, request, TimeSpan.FromSeconds(30));
        }

        public P Send<R, P>(ActorIdentity remoteActor, R request, TimeSpan timeout)
        {
            var envelope = new ActorMessageEnvelope<R>()
            {
                Message = request,
            };
            return this.Send<R, P>(remoteActor, envelope, timeout).Message;
        }

        public P Send<R, P>(string remoteActorType, string remoteActorName, R request)
        {
            return Send<R, P>(remoteActorType, remoteActorName, request, TimeSpan.FromSeconds(30));
        }

        public P Send<R, P>(string remoteActorType, string remoteActorName, R request, TimeSpan timeout)
        {
            var envelope = new ActorMessageEnvelope<R>()
            {
                Message = request,
            };
            return this.Send<R, P>(remoteActorType, remoteActorName, envelope, timeout).Message;
        }

        public P Send<R, P>(string remoteActorType, R request)
        {
            return Send<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public P Send<R, P>(string remoteActorType, R request, TimeSpan timeout)
        {
            var envelope = new ActorMessageEnvelope<R>()
            {
                Message = request,
            };
            return this.Send<R, P>(remoteActorType, envelope, timeout).Message;
        }

        #endregion

        #region Send Envelope

        public void Send<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            this.Actor.Send(remoteActor, message);
        }

        public void BeginSend<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            this.Actor.BeginSend(remoteActor, message);
        }

        public void Send<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            this.Actor.Send(remoteActorType, remoteActorName, message);
        }

        public void BeginSend<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            this.Actor.BeginSend(remoteActorType, remoteActorName, message);
        }

        public void Send<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            this.Actor.Send(remoteActorType, message);
        }

        public void BeginSend<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            this.Actor.BeginSend(remoteActorType, message);
        }

        #endregion

        #region Send Message

        public void Send<T>(ActorIdentity remoteActor, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.Send<T>(remoteActor, envelope);
        }

        public void BeginSend<T>(ActorIdentity remoteActor, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginSend<T>(remoteActor, envelope);
        }

        public void Send<T>(string remoteActorType, string remoteActorName, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.Send<T>(remoteActorType, remoteActorName, envelope);
        }

        public void BeginSend<T>(string remoteActorType, string remoteActorName, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginSend<T>(remoteActorType, remoteActorName, envelope);
        }

        public void Send<T>(string remoteActorType, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.Send<T>(remoteActorType, envelope);
        }

        public void BeginSend<T>(string remoteActorType, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginSend<T>(remoteActorType, envelope);
        }

        #endregion

        #region Reply Envelope

        public void Reply<T>(string channelIdentifier, ActorMessageEnvelope<T> message)
        {
            this.Actor.Reply(channelIdentifier, message);
        }

        public void BeginReply<T>(string channelIdentifier, ActorMessageEnvelope<T> message)
        {
            this.Actor.BeginReply(channelIdentifier, message);
        }

        #endregion

        #region Reply Message

        public void Reply<T>(string channelIdentifier, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.Reply<T>(channelIdentifier, envelope);
        }

        public void BeginReply<T>(string channelIdentifier, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginReply<T>(channelIdentifier, envelope);
        }

        #endregion

        #region Broadcast Envelope

        public void Broadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            this.Actor.Broadcast(remoteActorType, message);
        }

        public void BeginBroadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            this.Actor.BeginBroadcast(remoteActorType, message);
        }

        public void Broadcast<T>(IEnumerable<string> remoteActorTypes, ActorMessageEnvelope<T> message)
        {
            this.Actor.Broadcast(remoteActorTypes, message);
        }

        public void BeginBroadcast<T>(IEnumerable<string> remoteActorTypes, ActorMessageEnvelope<T> message)
        {
            this.Actor.BeginBroadcast(remoteActorTypes, message);
        }

        #endregion

        #region Broadcast Message

        public void Broadcast<T>(string remoteActorType, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.Broadcast<T>(remoteActorType, envelope);
        }

        public void BeginBroadcast<T>(string remoteActorType, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginBroadcast<T>(remoteActorType, envelope);
        }

        public void Broadcast<T>(IEnumerable<string> remoteActorTypes, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.Broadcast<T>(remoteActorTypes, envelope);
        }

        public void BeginBroadcast<T>(IEnumerable<string> remoteActorTypes, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginBroadcast<T>(remoteActorTypes, envelope);
        }

        #endregion
    }
}
