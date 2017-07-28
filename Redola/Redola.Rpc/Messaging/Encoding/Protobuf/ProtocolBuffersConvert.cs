using System;
using System.IO;

namespace Redola.Rpc
{
    public static class ProtocolBuffersConvert
    {
        public static byte[] SerializeObject(object obj)
        {
            byte[] data;
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.NonGeneric.Serialize(stream, obj);
                data = stream.ToArray();
            }

            return data;
        }

        public static byte[] SerializeObject<T>(T obj)
        {
            byte[] data;
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<T>(stream, obj);
                data = stream.ToArray();
            }

            return data;
        }

        public static object DeserializeObject(Type type, byte[] data)
        {
            return DeserializeObject(type, data, 0, data.Length);
        }

        public static object DeserializeObject(Type type, byte[] data, int dataOffset, int dataLength)
        {
            object obj = null;
            using (var stream = new MemoryStream(data, dataOffset, dataLength))
            {
                obj = ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream);
            }

            return obj;
        }

        public static T DeserializeObject<T>(byte[] data)
        {
            return DeserializeObject<T>(data, 0, data.Length);
        }

        public static T DeserializeObject<T>(byte[] data, int dataOffset, int dataLength)
        {
            T obj = default(T);
            using (var stream = new MemoryStream(data, dataOffset, dataLength))
            {
                obj = ProtoBuf.Serializer.Deserialize<T>(stream);
            }

            return obj;
        }
    }
}
