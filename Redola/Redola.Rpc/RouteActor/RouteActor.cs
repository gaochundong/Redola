using System;
using System.Collections.Generic;
using Logrila.Logging;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RouteActor : Actor
    {
        private ILog _log = Logger.Get<RouteActor>();
        private IActorMessageEncoder _encoder = null;
        private IActorMessageDecoder _decoder = null;
        private List<IRouteActorMessageHandler> _messageHandlers = new List<IRouteActorMessageHandler>();

        public RouteActor(
            ActorConfiguration configuration,
            IActorMessageEncoder encoder,
            IActorMessageDecoder decoder)
            : base(configuration)
        {
            _encoder = encoder;
            _decoder = decoder;
        }

        public IActorMessageEncoder Encoder { get { return _encoder; } }
        public IActorMessageDecoder Decoder { get { return _decoder; } }

        public void RegisterMessageHandler(IRouteActorMessageHandler messageHandler)
        {
            _messageHandlers.Add(messageHandler);
        }

        public IEnumerable<IRouteActorMessageHandler> GetMessageHandlers()
        {
            return _messageHandlers;
        }

        protected override void OnActorDataReceived(object sender, ActorDataReceivedEventArgs e)
        {
            var envelope = this.Decoder.DecodeEnvelope(e.Data, e.DataOffset, e.DataLength);

            bool handled = false;

            if (!handled)
            {
                foreach (var handler in GetMessageHandlers())
                {
                    if (handler.CanHandleMessage(envelope))
                    {
                        handler.HandleMessage(e.RemoteActor, envelope);
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled)
                _log.WarnFormat("OnActorDataReceived, cannot handle message [{0}] from remote actor [{1}].",
                    envelope.MessageType, e.RemoteActor);

            base.OnActorDataReceived(sender, e);
        }

        public void Send<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            Send(remoteActor, message.ToBytes(this.Encoder));
        }

        public void BeginSend<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActor, message.ToBytes(this.Encoder));
        }

        public IAsyncResult BeginSend<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message, AsyncCallback callback, object state)
        {
            return BeginSend(remoteActor, message.ToBytes(this.Encoder), callback, state);
        }

        public void Send<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            Send(remoteActorType, remoteActorName, message.ToBytes(this.Encoder));
        }

        public void BeginSend<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActorType, remoteActorName, message.ToBytes(this.Encoder));
        }

        public IAsyncResult BeginSend<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message, AsyncCallback callback, object state)
        {
            return BeginSend(remoteActorType, remoteActorName, message.ToBytes(this.Encoder), callback, state);
        }

        public void Send<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActorType, message.ToBytes(this.Encoder));
        }

        public void BeginSend<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActorType, message.ToBytes(this.Encoder));
        }

        public void Broadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            Broadcast(remoteActorType, message.ToBytes(this.Encoder));
        }

        public void BeginBroadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            BeginBroadcast(remoteActorType, message.ToBytes(this.Encoder));
        }
    }
}
