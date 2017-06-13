using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcActor
    {
        public static readonly IActorMessageEncoder DefaultActorMessageEncoder = new ActorMessageEncoder(new ProtocolBuffersMessageEncoder());
        public static readonly IActorMessageDecoder DefaultActorMessageDecoder = new ActorMessageDecoder(new ProtocolBuffersMessageDecoder());

        private BlockingRouteActor _localActor = null;

        public RpcActor()
            : this(AppConfigActorConfiguration.Load())
        {
        }

        public RpcActor(ActorConfiguration configuration)
            : this(configuration, DefaultActorMessageEncoder, DefaultActorMessageDecoder)
        {
        }

        public RpcActor(ActorConfiguration configuration, IActorMessageEncoder encoder, IActorMessageDecoder decoder)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            if (encoder == null)
                throw new ArgumentNullException("encoder");
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            _localActor = new BlockingRouteActor(configuration, encoder, decoder);
        }

        public BlockingRouteActor Actor { get { return _localActor; } }
        public IActorMessageEncoder Encoder { get { return _localActor.Encoder; } }
        public IActorMessageDecoder Decoder { get { return _localActor.Decoder; } }

        public void Bootup()
        {
            _localActor.Bootup();
        }

        public void Bootup(IActorDirectory directory)
        {
            _localActor.Bootup(directory);
        }

        public void Shutdown()
        {
            _localActor.Shutdown();
        }

        public void RegisterRpcService(RpcService service)
        {
            _localActor.RegisterMessageHandler(service);
        }

        #region Blocking Envelope

        public ActorMessageEnvelope<P> Send<R, P>(ActorIdentity remoteActor, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActor, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(ActorIdentity remoteActor, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return _localActor.SendMessage<R, P>(remoteActor, request, timeout);
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActorType, remoteActorName, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return _localActor.SendMessage<R, P>(remoteActorType, remoteActorName, request, timeout);
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return _localActor.SendMessage<R, P>(remoteActorType, request, timeout);
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
            _localActor.Send(remoteActor, message);
        }

        public void BeginSend<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginSend(remoteActor, message);
        }

        public void Send<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            _localActor.Send(remoteActorType, remoteActorName, message);
        }

        public void BeginSend<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginSend(remoteActorType, remoteActorName, message);
        }

        public void Send<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.Send(remoteActorType, message);
        }

        public void BeginSend<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginSend(remoteActorType, message);
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
            _localActor.Reply(channelIdentifier, message);
        }

        public void BeginReply<T>(string channelIdentifier, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginReply(channelIdentifier, message);
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
            _localActor.Broadcast(remoteActorType, message);
        }

        public void BeginBroadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginBroadcast(remoteActorType, message);
        }

        public void Broadcast<T>(IEnumerable<string> remoteActorTypes, ActorMessageEnvelope<T> message)
        {
            _localActor.Broadcast(remoteActorTypes, message);
        }

        public void BeginBroadcast<T>(IEnumerable<string> remoteActorTypes, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginBroadcast(remoteActorTypes, message);
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
            this.Broadcast(remoteActorTypes, envelope);
        }

        public void BeginBroadcast<T>(IEnumerable<string> remoteActorTypes, T message)
        {
            var envelope = new ActorMessageEnvelope<T>()
            {
                Message = message,
            };
            this.BeginBroadcast(remoteActorTypes, envelope);
        }

        #endregion
    }
}
