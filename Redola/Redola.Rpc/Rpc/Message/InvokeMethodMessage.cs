using System;
using ProtoBuf;

namespace Redola.Rpc
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class InvokeMethodMessage
    {
        public InvokeMethodMessage()
        {
        }

        public InvokeMethodMessage(string methodLocator, object[] methodArguments)
        {
            this.MethodLocator = methodLocator;
            this.MethodArguments = methodArguments;
        }

        [ProtoMember(10)]
        public string MethodLocator { get; set; }

        [ProtoIgnore]
        public object[] MethodArguments { get; set; }

        [ProtoMember(30)]
        public SerializableMethodArgument[] SerializableMethodArguments { get; set; }

        public void Serialize(IMethodArgumentEncoder encoder)
        {
            if (encoder == null)
                throw new ArgumentNullException("encoder");

            if (this.MethodArguments == null || this.MethodArguments.Length == 0)
                return;

            this.SerializableMethodArguments = new SerializableMethodArgument[this.MethodArguments.Length];
            for (int i = 0; i < this.MethodArguments.Length; i++)
            {
                var arg = this.MethodArguments[i];
                this.SerializableMethodArguments[i] = new SerializableMethodArgument()
                {
                    Type = arg.GetType().AssemblyQualifiedName,
                    Bytes = encoder.Encode(arg),
                };
            }
        }

        public void Deserialize(IMethodArgumentDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            if (this.SerializableMethodArguments == null || this.SerializableMethodArguments.Length == 0)
                return;

            this.MethodArguments = new object[this.SerializableMethodArguments.Length];
            for (int i = 0; i < this.SerializableMethodArguments.Length; i++)
            {
                var arg = this.SerializableMethodArguments[i];
                var type = Type.GetType(arg.Type);
                this.MethodArguments[i] = decoder.Decode(type, arg.Bytes, 0, arg.Bytes.Length);
            }
        }

        public override string ToString()
        {
            return string.Format("Locator[{0}], Arguments[{1}]",
                this.MethodLocator, this.MethodArguments == null ? 0 : this.MethodArguments.Length);
        }
    }
}
