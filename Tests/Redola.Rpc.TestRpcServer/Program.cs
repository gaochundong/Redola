using System;
using System.Text.RegularExpressions;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    class Program
    {
        static ILog _log;

        static Program()
        {
            NLogLogger.Use();
            _log = Logger.Get<Program>();
        }

        static void Main(string[] args)
        {
            var localActor = new RpcActor();

            var helloService = new HelloService(localActor, new CountableRateLimiter());
            var calcService = new CalcService(localActor, new CountableRateLimiter());
            var orderService = new OrderService(localActor, new CountableRateLimiter());

            localActor.RegisterRpcHandler(helloService);
            localActor.RegisterRpcHandler(calcService);
            localActor.RegisterRpcHandler(orderService);

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
                            NotifyOrderChanged(orderService);
                        }
                    }
                    else
                    {
                        _log.WarnFormat("Cannot parse the operation for input [{0}].", text);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                }
            }

            localActor.Shutdown();
        }

        private static void NotifyOrderChanged(OrderService orderService)
        {
            var notification = new ActorMessageEnvelope<OrderStatusChangedNotification>()
            {
                Message = new OrderStatusChangedNotification()
                {
                    OrderID = Guid.NewGuid().ToString(),
                    OrderStatus = 1,
                },
            };

            _log.DebugFormat("NotifyOrderChanged, notify order changed with MessageID[{0}].", notification.MessageID);
            orderService.NotifyOrderChanged(notification);
        }
    }
}
