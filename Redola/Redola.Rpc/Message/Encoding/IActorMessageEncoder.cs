namespace Redola.Rpc
{
    public interface IActorMessageEncoder
    {
        byte[] EncodeMessage<T>(T messageData);
        byte[] EncodeMessageEnvelope<T>(T messageData);
    }
}
