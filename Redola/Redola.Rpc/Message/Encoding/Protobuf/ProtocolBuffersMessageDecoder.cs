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

        public T DecodeMessage<T>(byte[] data, int dataLength)
        {
            return DecodeMessage<T>(data, 0, dataLength);
        }

        public T DecodeMessage<T>(byte[] data, int dataOffset, int dataLength)
        {
            if (data == null)
                throw new ArgumentNullException("The data which is to be deserialized cannot be null.");

            if (CompressionEnabled)
            {
                return ProtocolBuffersConvert.DeserializeObject<T>(GZipCompression.Decompress(data, dataOffset, dataLength));
            }
            else
            {
                return ProtocolBuffersConvert.DeserializeObject<T>(data, dataOffset, dataLength);
            }
        }
    }
}
