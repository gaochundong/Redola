using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    public class XmlConvert
    {
        public static string SerializeObject(object obj)
        {
            if (obj == null) return string.Empty;
            return SerializeObject(obj.GetType(), obj);
        }

        public static string SerializeObject<T>(T obj)
        {
            return SerializeObject(typeof(T), obj);
        }

        public static string SerializeObject(Type typeOfObject, object obj)
        {
            string str = null;
            MemoryStream ms = null;

            try
            {
                XmlSerializer xs = new XmlSerializer(typeOfObject);

                ms = new MemoryStream();
                using (var xtw = new XmlTextWriter(ms, System.Text.Encoding.UTF8)
                {
                    Formatting = System.Xml.Formatting.Indented
                })
                {
                    ms = null;
                    xs.Serialize(xtw, obj);
                    xtw.BaseStream.Seek(0, SeekOrigin.Begin);
                    using (StreamReader sr = new StreamReader(xtw.BaseStream))
                    {
                        str = sr.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (ms != null)
                {
                    ms.Close();
                    ms = null;
                }
            }

            return str;
        }

        public static T DeserializeObject<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return default(T);

            using (var reader = new StringReader(xml))
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                return (T)xs.Deserialize(reader);
            }
        }
    }
}
