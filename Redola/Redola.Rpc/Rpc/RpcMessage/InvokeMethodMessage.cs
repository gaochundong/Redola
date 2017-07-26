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

        [ProtoMember(20)]
        public object[] MethodArguments { get; set; }

        public override string ToString()
        {
            return string.Format("Locator[{0}], Arguments[{1}]",
                this.MethodLocator, this.MethodArguments == null ? 0 : this.MethodArguments.Length);
        }
    }
}
