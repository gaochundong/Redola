namespace Redola.Rpc
{
    public interface IActorMessageEncoder
    {
        byte[] EncodeMessage(object message);
        byte[] EncodeMessage<T>(T message);
        byte[] EncodeMessageEnvelope<T>(T message);
    }
}
