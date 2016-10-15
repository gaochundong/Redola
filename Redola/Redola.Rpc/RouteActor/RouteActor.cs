using System.Collections.Generic;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class RouteActor : Actor
    {
        private ILog _log = Logger.Get<RouteActor>();
        private List<IRouteActorMessageHandler> _messageHandlers = new List<IRouteActorMessageHandler>();

        public RouteActor(ActorConfiguration configuration)
            : base(configuration)
        {
        }

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
            var envelope = this.Decoder.Decode<MessageEnvelope>(e.Data, e.DataOffset, e.DataLength);

            if (envelope.Source == null)
                envelope.Source = Endpoint.CreateEndpoint();
            envelope.Source.AttachPath(e.RemoteActor.GetKey());

            if (envelope.Target != null)
                envelope.Target.DetachPath();

            bool handled = false;

            if (!handled)
            {
                if (envelope.Target != null)
                {
                    var target = envelope.Target.PeakPath();
                    if (!string.IsNullOrEmpty(target))
                    {
                        string remoteActorType, remoteActorName;
                        ActorDescription.Decode(target, out remoteActorType, out remoteActorName);
                        this.SendAsync(remoteActorType, remoteActorName, envelope.ToBytes(this.Encoder));
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
