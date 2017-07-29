using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestDynamicRpcServer
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

            var localXmlFileActorDirectoryPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ActorDirectory.xml";
            var localXmlFileActorDirectoryConfiguration = LocalXmlFileActorDirectoryConfiguration.Load(localXmlFileActorDirectoryPath);
            var localXmlFileActorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorDirectoryConfiguration);

            var catalog = new ServiceCatalogProvider();
            catalog.RegisterService<IHelloService>(new HelloService());
            catalog.RegisterService<ICalcService>(new CalcService());
            catalog.RegisterService<IOrderService>(new OrderService());

            var fixture = new RpcMethodFixture(
                new MethodLocatorExtractor(),
                new MethodArgumentEncoder(RpcActor.DefaultMessageEncoder),
                new MethodArgumentDecoder(RpcActor.DefaultMessageDecoder));

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

                        localXmlFileActorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorDirectoryConfiguration);
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
