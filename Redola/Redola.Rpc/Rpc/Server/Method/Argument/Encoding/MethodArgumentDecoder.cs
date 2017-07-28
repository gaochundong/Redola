using System;

namespace Redola.Rpc
{
    public class MethodArgumentDecoder : IMethodArgumentDecoder
    {
        private IMessageDecoder _decoder;

        public MethodArgumentDecoder(IMessageDecoder decoder)
        {
            _decoder = decoder;
        }

        public T Decode<T>(byte[] data, int offset, int count)
        {
            return _decoder.DecodeMessage<T>(data, offset, count);
        }

        public object Decode(Type type, byte[] data, int offset, int count)
        {
            return _decoder.DecodeMessage(type, data, offset, count);
        }
    }
}
