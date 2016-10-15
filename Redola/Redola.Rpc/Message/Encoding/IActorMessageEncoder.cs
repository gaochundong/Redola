namespace Redola.ActorModel
{
    public interface IActorMessageEncoder
    {
        byte[] EncodeMessage<T>(T messageData);
        byte[] EncodeMessageEnvelope<T>(T messageData);
    }
}
