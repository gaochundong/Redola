
namespace Redola.ActorModel.Serialization
{
    internal interface IMessageDecoder
    {
        T DecodeMessage<T>(byte[] data);
        T DecodeMessage<T>(byte[] data, int dataLength);
        T DecodeMessage<T>(byte[] data, int dataOffset, int dataLength);
    }
}
