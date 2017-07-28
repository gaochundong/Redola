using System;
using ProtoBuf;

namespace Redola.Rpc
{
    [ProtoContract(UseProtoMembersOnly = true)]
    public class InvokeMethodResponse
    {
        public InvokeMethodResponse()
        {
        }

        public InvokeMethodResponse(string methodLocator, object methodReturnValue)
        {
            this.MethodLocator = methodLocator;
            this.MethodReturnValue = methodReturnValue;
        }

        [ProtoMember(10)]
        public string MethodLocator { get; set; }

        [ProtoIgnore]
        public object MethodReturnValue { get; set; }

        [ProtoMember(30)]
        public SerializableMethodArgument SerializableMethodReturnValue { get; set; }

        public void Serialize(IMethodArgumentEncoder encoder)
        {
            if (encoder == null)
                throw new ArgumentNullException("encoder");

            if (this.MethodReturnValue == null)
                return;

            this.SerializableMethodReturnValue = new SerializableMethodArgument()
            {
                Type = this.MethodReturnValue.GetType().AssemblyQualifiedName,
                Bytes = encoder.Encode(this.MethodReturnValue),
            };
        }

        public void Deserialize(IMethodArgumentDecoder decoder)
        {
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            if (this.SerializableMethodReturnValue == null)
                return;

            var type = Type.GetType(this.SerializableMethodReturnValue.Type);
            this.MethodReturnValue = decoder.Decode(type, this.SerializableMethodReturnValue.Bytes, 0, this.SerializableMethodReturnValue.Bytes.Length);
        }

        public override string ToString()
        {
            return string.Format("Locator[{0}]", this.MethodLocator);
        }
    }
}
