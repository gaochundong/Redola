using System;
using Redola.ActorModel.Framing;
using Redola.ActorModel.Serialization;

namespace Redola.ActorModel
{
    public class ActorChannelConfiguration
    {
        private IActorFrameBuilder _frameBuilder;

        public ActorChannelConfiguration()
        {
            var messageEncoder = new XmlMessageEncoder();
            var messageDecoder = new XmlMessageDecoder();
            var controlFrameDataEncoder = new XmlActorControlFrameDataEncoder(messageEncoder);
            var controlFrameDataDecoder = new XmlActorControlFrameDataDecoder(messageDecoder);
            _frameBuilder = new ActorFrameBuilder(controlFrameDataEncoder, controlFrameDataDecoder);

            this.KeepAliveInterval = TimeSpan.FromSeconds(30);
            this.KeepAliveTimeout = TimeSpan.FromSeconds(10);
            this.KeepAliveEnabled = true;

            this.TransportConfiguration = new ActorTransportConfiguration();
        }

        public IActorFrameBuilder FrameBuilder { get { return _frameBuilder; } }
        public TimeSpan KeepAliveInterval { get; set; }
        public TimeSpan KeepAliveTimeout { get; set; }
        public bool KeepAliveEnabled { get; set; }

        public ActorTransportConfiguration TransportConfiguration { get; set; }
    }
}
