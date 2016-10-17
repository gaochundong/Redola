using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var actor = new RpcClientActor();

            try
            {
                actor.Bootup();

                var orderClient = new OrderClient(actor);
                actor.RegisterRpcClient(orderClient);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                    {
                        break;
                    }
                    else
                    {
                        int times = 0;
                        if (int.TryParse(text, out times))
                        {
                            for (int i = 0; i < times; i++)
                            {
                                PlaceOrder(log, actor);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }

            actor.Shutdown();
        }

        private static void PlaceOrder(ILog log, RpcClientActor actor)
        {
            var request = new ActorMessageEnvelope<PlaceOrderRequest>()
            {
                Message = new PlaceOrderRequest()
                {
                    Contract = new Order()
                    {
                        OrderID = Guid.NewGuid().ToString(),
                        ItemID = "Apple",
                        BuyCount = 100,
                    },
                },
            };

            log.DebugFormat("PlaceOrder, send place order request to server with MessageID[{0}].",
                request.MessageID);

            var response = actor.Send<PlaceOrderRequest, PlaceOrderResponse>("server", request);

            log.DebugFormat("PlaceOrder, receive place order response from server with MessageID[{0}] and CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
        }
    }
}
