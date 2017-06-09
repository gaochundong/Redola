using System;
using System.Text.RegularExpressions;
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

            var localActor = new RpcActor();

            var rateLimiter = new CountableRateLimiter();

            var helloService = new HelloService(localActor, rateLimiter);
            var calcService = new CalcService(localActor);
            var orderService = new OrderService(localActor, rateLimiter);

            localActor.RegisterRpcService(helloService);
            localActor.RegisterRpcService(calcService);
            localActor.RegisterRpcService(orderService);

            localActor.Bootup();

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                    {
                        break;
                    }
                    else if (text == "reconnect")
                    {
                        localActor.Shutdown();
                        localActor.Bootup();
                    }
                    else if (Regex.Match(text, @"^notify(\d*)$").Success)
                    {
                        var match = Regex.Match(text, @"notify(\d*)$");
                        int totalCalls = 0;
                        if (!int.TryParse(match.Groups[1].Value, out totalCalls))
                        {
                            totalCalls = 1;
                        }
                        for (int i = 0; i < totalCalls; i++)
                        {
                            NotifyOrderChanged(log, orderService);
                        }
                    }
                    else
                    {
                        log.WarnFormat("Cannot parse the operation for input [{0}].", text);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }

            localActor.Shutdown();
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

            log.DebugFormat("NotifyOrderChanged, notify order changed with MessageID[{0}].", notification.MessageID);
            orderService.NotifyOrderChanged(notification);
        }
    }
}
