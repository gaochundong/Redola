namespace Redola.Rpc
{
    public interface IActorMessageEncoder
    {
        byte[] EncodeMessage(object data);
        byte[] EncodeMessage<T>(T messageData);
        byte[] EncodeMessageEnvelope<T>(T messageData);
    }
}
