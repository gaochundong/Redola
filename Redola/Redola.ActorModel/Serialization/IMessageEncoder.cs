
namespace Redola.ActorModel.Serialization
{
    internal interface IMessageEncoder
    {
        byte[] EncodeMessage(object message);
        byte[] EncodeMessage<T>(T message);
    }
}
