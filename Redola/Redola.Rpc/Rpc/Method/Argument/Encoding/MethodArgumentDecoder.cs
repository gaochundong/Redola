using System;

namespace Redola.Rpc
{
    public class MethodArgumentDecoder : IMethodArgumentDecoder
    {
        private IObjectDecoder _decoder;

        public MethodArgumentDecoder(IObjectDecoder decoder)
        {
            _decoder = decoder;
        }

        public T Decode<T>(byte[] data, int offset, int count)
        {
            return _decoder.Decode<T>(data, offset, count);
        }

        public object Decode(Type type, byte[] data, int offset, int count)
        {
            return _decoder.Decode(type, data, offset, count);
        }
    }
}
