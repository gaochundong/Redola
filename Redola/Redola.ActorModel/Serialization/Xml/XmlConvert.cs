using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Redola.ActorModel.Serialization
{
    internal class XmlConvert
    {
        public static string SerializeObject(object obj)
        {
            if (obj == null) return string.Empty;
            return SerializeObject(obj.GetType(), obj);
        }

        public static string SerializeObject(object obj, XmlWriterSettings settings)
        {
            if (obj == null) return string.Empty;
            return SerializeObject(obj.GetType(), obj, settings);
        }

        public static string SerializeObject(object obj, XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
        {
            if (obj == null) return string.Empty;
            return SerializeObject(obj.GetType(), obj, settings, namespaces);
        }

        public static string SerializeObject<T>(T obj)
        {
            return SerializeObject(typeof(T), obj);
        }

        public static string SerializeObject<T>(T obj, XmlWriterSettings settings)
        {
            return SerializeObject(typeof(T), obj, settings);
        }

        public static string SerializeObject<T>(T obj, XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
        {
            return SerializeObject(typeof(T), obj, settings, namespaces);
        }

        public static string SerializeObject(Type typeOfObject, object obj)
        {
            return SerializeObject(typeOfObject, obj, null);
        }

        public static string SerializeObject(Type typeOfObject, object obj, XmlWriterSettings settings)
        {
            return SerializeObject(typeOfObject, obj, settings, null);
        }

        public static string SerializeObject(Type typeOfObject, object obj, XmlWriterSettings settings, XmlSerializerNamespaces namespaces)
        {
            if (settings == null)
            {
                settings = new XmlWriterSettings();
            }
            if (namespaces == null)
            {
                namespaces = new XmlSerializerNamespaces();
            }

            using (MemoryStream ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms, settings.Encoding))
            using (XmlWriter xw = XmlWriter.Create(sw, settings))
            {
                var xs = new XmlSerializer(typeOfObject);
                xs.Serialize(xw, obj, namespaces);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader sr = new StreamReader(ms, settings.Encoding))
                {
                    return sr.ReadToEnd();
                }
            }
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

        public static T Clone<T>(T obj)
        {
            return DeserializeObject<T>(SerializeObject(obj));
        }
    }
}
