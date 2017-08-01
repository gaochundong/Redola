using System;
using Happer;
using Happer.Hosting.Self;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.DynamicProxy.CastleIntegration;
using Redola.Rpc.ServiceDiscovery.XmlIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpRelay.XmlIntegration
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

            var localXmlFileServiceRegistryPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ServiceRegistry.xml";
            var serviceRegistry = LocalXmlFileServiceRegistry.Load(localXmlFileServiceRegistryPath);
            var serviceDiscovery = new LocalXmlFileServiceDiscovery(serviceRegistry);
            var serviceRetriever = new ServiceRetriever(serviceDiscovery);
            var serviceResolver = new ServiceResolver(serviceRetriever);
            var proxyGenerator = new ServiceProxyGenerator(serviceResolver);

            var rpcClient = new RpcClient(localActor, actorDirectory, proxyGenerator);

            var helloClient = rpcClient.Resolve<IHelloService>();
            var calcClient = rpcClient.Resolve<ICalcService>();

            rpcClient.Bootup();

            var container = new TestContainer();
            container.AddModule(new TestModule(helloClient, calcClient));

            var bootstrapper = new Bootstrapper();
            var engine = bootstrapper.BootWith(container);

            string uri = "http://localhost:3202/";
            var host = new SelfHost(engine, new Uri(uri));
            host.Start();
            Console.WriteLine("Server is listening on [{0}].", uri);

            Console.WriteLine("Type something to stop ...");
            Console.ReadKey();

            host.Stop();
            rpcClient.Shutdown();
            Console.WriteLine("Stopped. Goodbye!");
        }
    }
}
