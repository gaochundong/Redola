using System;

namespace Redola.Rpc
{
    public class ActorMessageDecoder : IActorMessageDecoder
    {
        private IObjectDecoder _decoder;

        public ActorMessageDecoder(IObjectDecoder decoder)
        {
            _decoder = decoder;
        }

        public ActorMessageEnvelope DecodeEnvelope(byte[] data, int offset, int count)
        {
            return _decoder.Decode<ActorMessageEnvelope>(data, offset, count);
        }

        public T DecodeMessage<T>(byte[] data, int offset, int count)
        {
            return _decoder.Decode<T>(data, offset, count);
        }

        public T DecodeEnvelopeMessage<T>(byte[] data, int offset, int count)
        {
            var envelope = DecodeEnvelope(data, offset, count);
            return DecodeMessage<T>(envelope.MessageData, 0, envelope.MessageData.Length);
        }

        public object DecodeMessage(Type type, byte[] data, int offset, int count)
        {
            return _decoder.Decode(type, data, offset, count);
        }
    }
}
