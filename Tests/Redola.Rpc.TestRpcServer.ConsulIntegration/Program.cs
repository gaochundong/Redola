using System;
using Consul;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
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

            var serviceRegistry = new ConsulServiceRegistry(consul);
            var serviceDirectory = new ConsulServiceDirectory(serviceRegistry);

            var serviceCatalog = new ServiceCatalogProvider();
            serviceCatalog.RegisterService<IHelloService>(new HelloService());
            serviceCatalog.RegisterService<ICalcService>(new CalcService());
            serviceCatalog.RegisterService<IOrderService>(new OrderService());

            var rpcServer = new RpcServer(localActor, actorDirectory, serviceCatalog, serviceDirectory);

            rpcServer.Bootup();

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
                        rpcServer.Shutdown();
                        rpcServer.Bootup();
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

            rpcServer.Shutdown();
        }
    }
}
