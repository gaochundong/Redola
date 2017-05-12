using System;

namespace Redola.Rpc
{
    public class RequestResponseMessageContract : RpcMessageContract
    {
        public RequestResponseMessageContract(Type request, Type response)
            : base(response)
        {
            if (request == null)
                throw new ArgumentNullException("request");
            if (response == null)
                throw new ArgumentNullException("response");

            this.RequestMessageType = request;
            this.ResponseMessageType = response;

            this.IsOneWay = false;
        }

        public Type RequestMessageType { get; protected set; }
        public Type ResponseMessageType { get; protected set; }
    }
}
