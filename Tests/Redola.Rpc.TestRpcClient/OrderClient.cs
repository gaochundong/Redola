using System;
using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    public class OrderClient : RpcClient
    {
        private ILog _log = Logger.Get<OrderClient>();

        public OrderClient(RpcClientActor localActor)
            : base(localActor)
        {
        }

        protected override IEnumerable<Tuple<Type, Type>> RegisterRpcMessages()
        {
            var messages = new List<Tuple<Type, Type>>();

            messages.Add(new Tuple<Type, Type>(typeof(PlaceOrderRequest), typeof(PlaceOrderResponse)));

            return messages;
        }
    }
}
