using System;

namespace Redola.Rpc
{
    public class RequestResponseMessageContract : RpcMessageContract
    {
        public RequestResponseMessageContract()
            : base()
        {
            this.IsOneWay = false;
        }

        public RequestResponseMessageContract(Type requestMessageType, Type responseMessageType)
            : base(responseMessageType)
        {
            this.IsOneWay = false;
        }

        public Type RequestMessageType { get; set; }
        public Type ResponseMessageType { get; set; }
    }
}
