using System;
using System.Text;

namespace Redola.ActorModel.Serialization
{
    internal class XmlMessageDecoder : IMessageDecoder
    {
        public XmlMessageDecoder()
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
            {
                throw new ArgumentNullException("data", "The data which is to be deserialized cannot be null.");
            }
            if (dataOffset < 0 || dataOffset > data.Length)
            {
                throw new ArgumentOutOfRangeException("dataOffset");
            }
            if (dataLength < 0 || dataLength > (data.Length - dataOffset))
            {
                throw new ArgumentOutOfRangeException("dataLength");
            }

            if (CompressionEnabled)
            {
                string xml = Encoding.UTF8.GetString(GZipCompression.Decompress(data, dataOffset, dataLength));
                return XmlConvert.DeserializeObject<T>(xml);
            }
            else
            {
                string xml = Encoding.UTF8.GetString(data, dataOffset, dataLength);
                return XmlConvert.DeserializeObject<T>(xml);
            }
        }
    }
}
