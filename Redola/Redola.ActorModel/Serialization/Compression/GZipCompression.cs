using System.IO;
using System.IO.Compression;

namespace Redola.ActorModel.Serialization
{
    internal static class GZipCompression
    {
        public static byte[] Compress(byte[] raw)
        {
            return Compress(raw, 0, raw.Length);
        }

        public static byte[] Compress(byte[] raw, int offset, int count)
        {
            using (var memory = new MemoryStream())
            {
                using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
                {
                    gzip.Write(raw, offset, count);
                }
                return memory.ToArray();
            }
        }

        public static byte[] Decompress(byte[] gzip)
        {
            return Decompress(gzip, 0, gzip.Length);
        }

        public static byte[] Decompress(byte[] gzip, int offset, int count)
        {
            using (var input = new MemoryStream(gzip, offset, count))
            using (var stream = new GZipStream(input, CompressionMode.Decompress))
            {
                const int size = 1024;
                byte[] buffer = new byte[size];
                using (var memory = new MemoryStream())
                {
                    int readCount = 0;
                    do
                    {
                        readCount = stream.Read(buffer, 0, size);
                        if (readCount > 0)
                        {
                            memory.Write(buffer, 0, readCount);
                        }
                    }
                    while (readCount > 0);

                    return memory.ToArray();
                }
            }
        }
    }
}
