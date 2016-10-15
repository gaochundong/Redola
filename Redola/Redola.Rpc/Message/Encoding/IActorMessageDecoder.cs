namespace Redola.ActorModel
{
    public interface IActorMessageDecoder
    {
        IActorMessageEnvelope DecodeEnvelope(byte[] data, int offset, int count);
        T DecodeMessage<T>(byte[] data, int offset, int count);
        T DecodeEnvelopeMessage<T>(byte[] data, int offset, int count);
    }
}
