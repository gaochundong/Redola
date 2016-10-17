using System;
using System.Collections.Generic;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    public class OrderService : RpcService
    {
        private ILog _log = Logger.Get<OrderService>();

        public OrderService(RpcActor localActor)
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
