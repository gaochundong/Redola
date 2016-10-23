using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var actor = new RpcActor();

            actor.Bootup();

            var rateLimiter = new RateLimiter();

            var helloService = new HelloService(actor, rateLimiter);
            var calcService = new CalcService(actor);
            var orderService = new OrderService(actor, rateLimiter);

            actor.RegisterRpcService(helloService);
            actor.RegisterRpcService(calcService);
            actor.RegisterRpcService(orderService);

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
                                NotifyOrderChanged(log, orderService);
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

        private static void NotifyOrderChanged(ILog log, OrderService orderService)
        {
            var notification = new ActorMessageEnvelope<OrderStatusChangedNotification>()
            {
                Message = new OrderStatusChangedNotification()
                {
                    OrderID = Guid.NewGuid().ToString(),
                    OrderStatus = 1,
                },
            };

            log.DebugFormat("NotifyOrderChanged, notify order changed with MessageID[{0}].",
                notification.MessageID);
            orderService.NotifyOrderChanged(notification);
        }
    }
}
