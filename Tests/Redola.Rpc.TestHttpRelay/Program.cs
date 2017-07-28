using System;
using Happer;
using Happer.Hosting.Self;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.DynamicProxy.CastleIntegration;
using Redola.Rpc.ServiceDiscovery.XmlIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpRelay
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
            var localXmlFileActorPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ActorConfiguration.xml";
            var localXmlFileActorConfiguration = LocalXmlFileActorConfiguration.Load(localXmlFileActorPath);
            var localXmlFileActorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorConfiguration);
            var localActor = new RpcActor(localXmlFileActorConfiguration);

            var localXmlFileServiceRegistryPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ServiceRegistry.xml";
            var serviceDiscovery = new LocalXmlFileServiceDiscovery(localXmlFileServiceRegistryPath);
            var serviceRetriever = new ServiceRetriever(serviceDiscovery);
            var serviceResolver = new ServiceResolver(serviceRetriever);
            var proxyGenerator = new ServiceProxyGenerator(serviceResolver);

            var fixture = new RpcMethodFixture(
                new MethodLocatorExtractor(),
                new MethodArgumentEncoder(RpcActor.DefaultMessageEncoder),
                new MethodArgumentDecoder(RpcActor.DefaultMessageDecoder));

            var rpcClient = new RpcClient(localActor, proxyGenerator, fixture);

            var helloClient = rpcClient.Resolve<IHelloService>();
            var calcClient = rpcClient.Resolve<ICalcService>();

            localActor.Bootup(localXmlFileActorDirectory);

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
            Console.WriteLine("Stopped. Goodbye!");
        }
    }
}
