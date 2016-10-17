using System.Collections.Generic;
using Logrila.Logging;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RouteActor : Actor
    {
        private ILog _log = Logger.Get<RouteActor>();
        private IActorMessageEncoder _encoder;
        private IActorMessageDecoder _decoder;
        private List<IRouteActorMessageHandler> _messageHandlers = new List<IRouteActorMessageHandler>();

        public RouteActor(
            ActorConfiguration configuration,
            IActorMessageEncoder encoder,
            IActorMessageDecoder decoder)
            : base(configuration)
        {
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
    }
}
