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
            var localXmlFileActorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorRegistry);

            var catalog = new ServiceCatalogProvider();
            catalog.RegisterService<IHelloService>(new HelloService());
            catalog.RegisterService<ICalcService>(new CalcService());
            catalog.RegisterService<IOrderService>(new OrderService());

            var fixture = new RpcMethodFixture(
                new MethodLocatorExtractor(),
                new MethodArgumentEncoder(RpcActor.DefaultObjectEncoder),
                new MethodArgumentDecoder(RpcActor.DefaultObjectDecoder));

            var rpcServer = new RpcServer(localActor, catalog, fixture);

            localActor.Bootup(localXmlFileActorDirectory);

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

                        localXmlFileActorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorRegistry);
                        localActor.Bootup(localXmlFileActorDirectory);
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
    }
}
