using System;

namespace Redola.Rpc
{
    public class ReceiveMessageContract : RpcMessageContract
    {
        public ReceiveMessageContract(Type messageType)
            : base(messageType)
        {
            if (messageType == null)
                throw new ArgumentNullException("messageType");

            this.ReceiveMessageType = messageType;

            this.IsOneWay = true;
        }

        public Type ReceiveMessageType { get; protected set; }
    }
}
