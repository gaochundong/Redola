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

        protected override void OnActorChannelDataReceived(object sender, ActorChannelDataReceivedEventArgs e)
        {
            var envelope = this.Decoder.DecodeEnvelope(e.Data, e.DataOffset, e.DataLength);

            if (envelope.SourceEndpoint == null)
                envelope.SourceEndpoint = ActorEndpoint.CreateEndpoint();
            envelope.SourceEndpoint.AttachPath(e.RemoteActor.GetKey());

            if (envelope.TargetEndpoint != null)
                envelope.TargetEndpoint.DetachPath();

            bool handled = false;

            if (!handled)
            {
                if (envelope.TargetEndpoint != null)
                {
                    var target = envelope.TargetEndpoint.PeakPath();
                    if (!string.IsNullOrEmpty(target))
                    {
                        string remoteActorType, remoteActorName;
                        ActorIdentity.Decode(target, out remoteActorType, out remoteActorName);
                        this.Send(remoteActorType, remoteActorName, envelope.ToBytes(this.Encoder));
                        handled = true;
                    }
                }
            }

            if (!handled)
            {
                foreach (var handler in GetMessageHandlers())
                {
                    if (handler.CanHandleMessage(envelope))
                    {
                        handler.HandleMessage(new ActorSender(e.RemoteActor, e.ChannelIdentifier), envelope);
                        handled = true;
                        break;
                    }
                }
            }

            if (!handled)
                _log.WarnFormat("OnActorChannelDataReceived, cannot handle message [{0}] from sender [{1}].",
                    envelope.MessageType, new ActorSender(e.RemoteActor, e.ChannelIdentifier));

            base.OnActorChannelDataReceived(sender, e);
        }

        #region Send

        public void Send<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            Send(remoteActor, message.ToBytes(this.Encoder));
        }

        public void BeginSend<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActor, message.ToBytes(this.Encoder));
        }

        public void Send<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            Send(remoteActorType, remoteActorName, message.ToBytes(this.Encoder));
        }

        public void BeginSend<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActorType, remoteActorName, message.ToBytes(this.Encoder));
        }

        public void Send<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            Send(remoteActorType, message.ToBytes(this.Encoder));
        }

        public void BeginSend<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            BeginSend(remoteActorType, message.ToBytes(this.Encoder));
        }

        #endregion

        #region Reply

        public void Reply<T>(string channelIdentifier, ActorMessageEnvelope<T> message)
        {
            Reply(channelIdentifier, message.ToBytes(this.Encoder));
        }

        public void BeginReply<T>(string channelIdentifier, ActorMessageEnvelope<T> message)
        {
            BeginReply(channelIdentifier, message.ToBytes(this.Encoder));
        }

        #endregion

        #region Broadcast

        public void Broadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            Broadcast(remoteActorType, message.ToBytes(this.Encoder));
        }

        public void BeginBroadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            BeginBroadcast(remoteActorType, message.ToBytes(this.Encoder));
        }

        public void Broadcast<T>(IEnumerable<string> remoteActorTypes, ActorMessageEnvelope<T> message)
        {
            Broadcast(remoteActorTypes, message.ToBytes(this.Encoder));
        }

        public void BeginBroadcast<T>(IEnumerable<string> remoteActorTypes, ActorMessageEnvelope<T> message)
        {
            BeginBroadcast(remoteActorTypes, message.ToBytes(this.Encoder));
        }

        #endregion
    }
}
