using ProtoBuf;

namespace Redola.Rpc
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class SerializableMethodArgument
    {
        public SerializableMethodArgument()
        {
        }

        public SerializableMethodArgument(string type, byte[] bytes)
        {
            this.Type = type;
            this.Bytes = bytes;
        }

        [ProtoMember(10)]
        public string Type { get; set; }

        [ProtoMember(20)]
        public byte[] Bytes { get; set; }

        public override string ToString()
        {
            return string.Format("Type[{0}], Bytes[{1}]",
                this.Type, this.Bytes == null ? 0 : this.Bytes.Length);
        }
    }
}
