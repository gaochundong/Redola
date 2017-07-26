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

        [ProtoMember(20)]
        public object MethodReturnValue { get; set; }

        public override string ToString()
        {
            return string.Format("Locator[{0}]", this.MethodLocator);
        }
    }
}
