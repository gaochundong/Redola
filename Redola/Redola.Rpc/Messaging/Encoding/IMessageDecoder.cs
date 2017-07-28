using System;

namespace Redola.Rpc
{
    public interface IMessageDecoder
    {
        T DecodeMessage<T>(byte[] data);
        T DecodeMessage<T>(byte[] data, int count);
        T DecodeMessage<T>(byte[] data, int offset, int count);
        object DecodeMessage(Type type, byte[] data, int offset, int count);
    }
}
