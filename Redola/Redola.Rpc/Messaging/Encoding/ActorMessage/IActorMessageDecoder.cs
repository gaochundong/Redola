using System;

namespace Redola.Rpc
{
    public interface IActorMessageDecoder
    {
        ActorMessageEnvelope DecodeEnvelope(byte[] data, int offset, int count);
        T DecodeMessage<T>(byte[] data, int offset, int count);
        T DecodeEnvelopeMessage<T>(byte[] data, int offset, int count);
        object DecodeMessage(Type type, byte[] data, int offset, int count);
    }
}
