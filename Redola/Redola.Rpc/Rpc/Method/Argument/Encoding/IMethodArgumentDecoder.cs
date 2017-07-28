using System;

namespace Redola.Rpc
{
    public interface IMethodArgumentDecoder
    {
        T Decode<T>(byte[] data, int offset, int count);
        object Decode(Type type, byte[] data, int offset, int count);
    }
}
