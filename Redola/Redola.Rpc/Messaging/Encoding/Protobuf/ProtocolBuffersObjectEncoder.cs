using System;

namespace Redola.Rpc
{
    public class ProtocolBuffersObjectEncoder : IObjectEncoder
    {
        public ProtocolBuffersObjectEncoder()
        {
            this.CompressionEnabled = true;
        }

        public bool CompressionEnabled { get; set; }

        public byte[] Encode(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("The object to be serialized cannot be null.");

            var raw = ProtocolBuffersConvert.SerializeObject(obj);
            if (CompressionEnabled)
            {
                return GZipCompression.Compress(raw);
            }
            else
            {
                return raw;
            }
        }

        public byte[] Encode<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("The object to be serialized cannot be null.");

            var raw = ProtocolBuffersConvert.SerializeObject<T>(obj);
            if (CompressionEnabled)
            {
                return GZipCompression.Compress(raw);
            }
            else
            {
                return raw;
            }
        }
    }
}
