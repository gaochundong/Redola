using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.ServiceDiscovery.XmlIntegration;
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
            var localXmlFileLocalActorPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\LocalActor.xml";
            var localXmlFileLocalActorConfiguration = LocalXmlFileActorConfiguration.Load(localXmlFileLocalActorPath);
            var localActor = new RpcActor(localXmlFileLocalActorConfiguration);

            var localXmlFileActorRegistryPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ActorRegistry.xml";
            var localXmlFileActorRegistry = LocalXmlFileActorRegistry.Load(localXmlFileActorRegistryPath);
            var actorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorRegistry);

            var serviceCatalog = new ServiceCatalogProvider();
            serviceCatalog.RegisterService<IHelloService>(new HelloService());
            serviceCatalog.RegisterService<ICalcService>(new CalcService());
            serviceCatalog.RegisterService<IOrderService>(new OrderService());

            var rpcServer = new RpcServer(localActor, actorDirectory, serviceCatalog);

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
