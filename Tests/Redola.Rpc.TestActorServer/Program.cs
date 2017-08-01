using System;
using System.Text.RegularExpressions;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestActorServer
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
            var localActorConfiguration = AppConfigActorConfiguration.Load();
            var localActor = new RpcActor(localActorConfiguration);

            var helloService = new HelloService(localActor, new CountableRateLimiter());
            var calcService = new CalcService(localActor, new CountableRateLimiter());
            var orderService = new OrderService(localActor, new CountableRateLimiter());
            var orderEventClient = new OrderEventClient(localActor);

            localActor.RegisterRpcHandler(helloService);
            localActor.RegisterRpcHandler(calcService);
            localActor.RegisterRpcHandler(orderService);
            localActor.RegisterRpcHandler(orderEventClient);

            var directoryConfiguration = AppConfigCenterActorDirectoryConfiguration.Load();
            var directory = new CenterActorDirectory(directoryConfiguration);
            localActor.Bootup(directory);

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

                        directory = new CenterActorDirectory(directoryConfiguration);
                        localActor.Bootup(directory);
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
                            NotifyOrderDelivered(orderEventClient);
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

        private static void NotifyOrderDelivered(IOrderEventService orderEventClient)
        {
            var notification = new OrderDeliveredNotification()
            {
                OrderID = Guid.NewGuid().ToString(),
                OrderStatus = 1,
            };

            _log.DebugFormat("NotifyOrderDelivered, order [{0}] delivered.", notification.OrderID);
            orderEventClient.OrderDelivered(notification);
        }
    }
}
