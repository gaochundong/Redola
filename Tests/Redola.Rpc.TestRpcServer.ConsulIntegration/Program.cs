using System;
using System.Text.RegularExpressions;
using Consul;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.DynamicProxy.CastleIntegration;
using Redola.Rpc.ServiceDiscovery.ConsulIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer.ConsulIntegration
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
            var localXmlFileLocalActorPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\LocalActor.xml";
            var localXmlFileLocalActorConfiguration = LocalXmlFileActorConfiguration.Load(localXmlFileLocalActorPath);
            var localActor = new RpcActor(localXmlFileLocalActorConfiguration);

            var consul = new ConsulClient((c) =>
            {
                c.Address = new Uri("http://localhost:8881");
            });
            var actorRegistry = new ConsulActorRegistry(consul);
            var actorDirectory = new ConsulActorDirectory(actorRegistry);

            var serviceCatalog = new ServiceCatalogProvider();
            serviceCatalog.RegisterService<IHelloService>(new HelloService());
            serviceCatalog.RegisterService<ICalcService>(new CalcService());
            serviceCatalog.RegisterService<IOrderService>(new OrderService());

            var serviceRegistry = new ConsulServiceRegistry(consul);
            var serviceDirectory = new ConsulServiceDirectory(serviceRegistry);
            var serviceDiscovery = new ConsulServiceDiscovery(serviceRegistry);
            var serviceRetriever = new ServiceRetriever(serviceDiscovery);
            var serviceResolver = new ServiceResolver(serviceRetriever);
            var proxyGenerator = new ServiceProxyGenerator(serviceResolver);

            var rpcNode = new RpcNode(localActor, actorDirectory, serviceCatalog, serviceDirectory, proxyGenerator);

            var orderEventClient = rpcNode.Resolve<IOrderEventService>();

            rpcNode.Bootup();

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
                        rpcNode.Shutdown();
                        rpcNode.Bootup();
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

            rpcNode.Shutdown();
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
