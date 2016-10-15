using System;
using System.Text;

namespace Redola.ActorModel.Serialization
{
    internal class XmlMessageEncoder : IMessageEncoder
    {
        public XmlMessageEncoder()
        {
            this.CompressionEnabled = true;
        }

        public bool CompressionEnabled { get; set; }

        public byte[] EncodeMessage(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message", "The message which is to be serialized cannot be null.");

            if (CompressionEnabled)
            {
                var xml = XmlConvert.SerializeObject(message);
                return GZipCompression.Compress(Encoding.UTF8.GetBytes(xml));
            }
            else
            {
                var xml = XmlConvert.SerializeObject(message);
                return Encoding.UTF8.GetBytes(xml);
            }
        }

        public byte[] EncodeMessage<T>(T message)
        {
            if (message == null)
                throw new ArgumentNullException("message", "The message which is to be serialized cannot be null.");

            if (CompressionEnabled)
            {
                var xml = XmlConvert.SerializeObject(message);
                return GZipCompression.Compress(Encoding.UTF8.GetBytes(xml));
            }
            else
            {
                var xml = XmlConvert.SerializeObject(message);
                return Encoding.UTF8.GetBytes(xml);
            }
        }
    }
}
