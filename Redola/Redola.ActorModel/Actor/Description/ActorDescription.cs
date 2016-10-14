using System;
using ProtoBuf;

namespace Redola.ActorModel
{
    [ProtoContract(SkipConstructor = false, UseProtoMembersOnly = true)]
    public class ActorDescription : IEquatable<ActorDescription>
    {
        public ActorDescription()
        {
        }

        public ActorDescription(string type, string name)
        {
            this.Type = type;
            this.Name = name;
        }

        [ProtoMember(10)]
        public string Name { get; set; }
        [ProtoMember(20)]
        public string Type { get; set; }
        [ProtoMember(30)]
        public string Address { get; set; }
        [ProtoMember(40)]
        public string Port { get; set; }

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
            return string.Format("Name[{0}], Type[{1}], Address[{2}], Port[{3}]",
                Name, Type, Address, Port);
        }

        public override int GetHashCode()
        {
            return this.GetKey().GetHashCode();
        }

        public bool Equals(ActorDescription other)
        {
            if (object.ReferenceEquals(other, null)) return false;
            if (object.ReferenceEquals(this, other)) return true;

            return (StringComparer.OrdinalIgnoreCase.Compare(this.GetKey(), other.GetKey()) == 0);
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;

            return Equals(obj as ActorDescription);
        }

        public static bool operator ==(ActorDescription x, ActorDescription y)
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

        public static bool operator !=(ActorDescription x, ActorDescription y)
        {
            return !(x == y);
        }
    }
}
