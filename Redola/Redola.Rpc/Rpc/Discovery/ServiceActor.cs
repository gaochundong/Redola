using System;
using System.Xml.Serialization;

namespace Redola.Rpc
{
    [Serializable]
    [XmlRoot]
    public class ServiceActor : IEquatable<ServiceActor>
    {
        public ServiceActor()
        {
        }

        public ServiceActor(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Type { get; set; }

        public string GetKey()
        {
            return GetKey(this.Type, this.Name);
        }

        public static string GetKey(string type, string name)
        {
            return string.Format("{0}@{1}", name, type);
        }

        public static bool TryDecode(string key, out string type, out string name)
        {
            type = null;
            name = null;

            if (string.IsNullOrEmpty(key))
                return false;

            var pair = key.Split('@');
            if (pair.Length != 2)
                return false;

            name = pair[0];
            type = pair[1];
            return true;
        }

        public static void Decode(string key, out string type, out string name)
        {
            type = null;
            name = null;

            if (!TryDecode(key, out type, out name))
                throw new ArgumentException(string.Format("Invalid actor key [{0}].", key));
        }

        public override string ToString()
        {
            return string.Format("Name[{0}], Type[{1}]", Name, Type);
        }

        public override int GetHashCode()
        {
            return this.GetKey().GetHashCode();
        }

        public bool Equals(ServiceActor other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return (StringComparer.OrdinalIgnoreCase.Compare(this.GetKey(), other.GetKey()) == 0);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;

            return Equals(obj as ServiceActor);
        }

        public static bool operator ==(ServiceActor x, ServiceActor y)
        {
            if (object.ReferenceEquals(x, null))
            {
                if (object.ReferenceEquals(y, null))
                    return true;
                else
                    return false;
            }
            else
            {
                return x.Equals(y);
            }
        }

        public static bool operator !=(ServiceActor x, ServiceActor y)
        {
            return !(x == y);
        }
    }
}
