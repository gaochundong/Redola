using Redola.ActorModel.Serialization;

namespace Redola.ActorModel.Framing
{
    internal class XmlActorControlFrameDataEncoder : IActorControlFrameDataEncoder
    {
        private IMessageEncoder _encoder;

        public XmlActorControlFrameDataEncoder(IMessageEncoder encoder)
        {
            _encoder = encoder;
        }

        public byte[] EncodeFrameData<T>(T frameData)
        {
            return _encoder.EncodeMessage(frameData);
        }
    }
}
