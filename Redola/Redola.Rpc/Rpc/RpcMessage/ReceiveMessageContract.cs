using System;

namespace Redola.Rpc
{
    public class ReceiveMessageContract : RpcMessageContract
    {
        public ReceiveMessageContract()
            : base()
        {
            this.IsOneWay = true;
        }

        public ReceiveMessageContract(Type messageType)
            : base(messageType)
        {
            this.IsOneWay = true;
        }
    }
}
