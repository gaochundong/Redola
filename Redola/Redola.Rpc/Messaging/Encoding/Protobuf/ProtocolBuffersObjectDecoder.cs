using System;

namespace Redola.Rpc
{
    public class ProtocolBuffersObjectDecoder : IObjectDecoder
    {
        public ProtocolBuffersObjectDecoder()
        {
            this.CompressionEnabled = true;
        }

        public bool CompressionEnabled { get; set; }

        public T Decode<T>(byte[] data)
        {
            return Decode<T>(data, data.Length);
        }

        public T Decode<T>(byte[] data, int count)
        {
            return Decode<T>(data, 0, count);
        }

        public T Decode<T>(byte[] data, int offset, int count)
        {
            if (data == null)
                throw new ArgumentNullException("The data to be deserialized cannot be null.");

            if (CompressionEnabled)
            {
                return ProtocolBuffersConvert.DeserializeObject<T>(GZipCompression.Decompress(data, offset, count));
            }
            else
            {
                return ProtocolBuffersConvert.DeserializeObject<T>(data, offset, count);
            }
        }

        public object Decode(Type type, byte[] data, int offset, int count)
        {
            if (type == null)
                throw new ArgumentNullException("The type to be deserialized cannot be null.");
            if (data == null)
                throw new ArgumentNullException("The data to be deserialized cannot be null.");

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
