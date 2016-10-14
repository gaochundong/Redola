namespace Redola.ActorModel
{
    public interface IActorMessageEncoder
    {
        byte[] EncodeMessage<T>(T messageData);
        byte[] Encode<T>(T messageData);
    }
}
