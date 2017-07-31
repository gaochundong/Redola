using System;

namespace Redola.Rpc
{
    public interface IObjectDecoder
    {
        T Decode<T>(byte[] data);
        T Decode<T>(byte[] data, int count);
        T Decode<T>(byte[] data, int offset, int count);
        object Decode(Type type, byte[] data, int offset, int count);
    }
}
