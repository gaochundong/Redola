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

            var configruation = new RpcActorConfiguration();
            configruation.Build();

            var actor = new RpcActor(configruation);

            try
            {
                actor.Bootup();
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

        private static void PlaceOrder(ILog log, RpcActor actor)
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

            log.DebugFormat("PlaceOrder, request order contract [{0}] to server.", request.Message.Contract.OrderID);
            var response = actor.SendMessage<PlaceOrderRequest, PlaceOrderResponse>("server", request);
            log.DebugFormat("PlaceOrder, response order [{0}] from server.", "");
        }
    }
}
