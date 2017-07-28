using System;

namespace Redola.Rpc
{
    public class ProtocolBuffersMessageDecoder : IMessageDecoder
    {
        public ProtocolBuffersMessageDecoder()
        {
            this.CompressionEnabled = true;
        }

        public bool CompressionEnabled { get; set; }

        public T DecodeMessage<T>(byte[] data)
        {
            return DecodeMessage<T>(data, data.Length);
        }

        public T DecodeMessage<T>(byte[] data, int count)
        {
            return DecodeMessage<T>(data, 0, count);
        }

        public T DecodeMessage<T>(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException("The data which is to be deserialized cannot be null.");

            if (CompressionEnabled)
            {
                return ProtocolBuffersConvert.DeserializeObject<T>(GZipCompression.Decompress(data, offset, count));
            }
            else
            {
                return ProtocolBuffersConvert.DeserializeObject<T>(data, offset, count);
            }
        }

        public object DecodeMessage(Type type, byte[] data, int offset, int count)
        {
            if (type == null)
                throw new ArgumentNullException("The type which is to be deserialized cannot be null.");
            if (data == null)
                throw new ArgumentNullException("The data which is to be deserialized cannot be null.");

            if (CompressionEnabled)
            {
                return ProtocolBuffersConvert.DeserializeObject(type, GZipCompression.Decompress(data, offset, count));
            }
            else
            {
                return ProtocolBuffersConvert.DeserializeObject(type, data, offset, count);
            }
        }
    }
}
