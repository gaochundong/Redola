namespace Redola.ActorModel
{
    public interface IActorMessageDecoder
    {
        IActorMessageEnvelope DecodeEnvelope(byte[] data, int offset, int count);
        T DecodeMessage<T>(byte[] data, int offset, int count);
        T Decode<T>(byte[] data, int offset, int count);
    }
}
