using System;
using Happer;
using Happer.Hosting.Self;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.DynamicProxy;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var localXmlFilePath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ActorConfiguration.xml";
            var localXmlFileActorConfiguration = LocalXmlFileActorConfiguration.Load(localXmlFilePath);
            var localXmlFileActorDirectory = new LocalXmlFileActorDirectory(localXmlFileActorConfiguration);

            var localActor = new RpcActor(localXmlFileActorConfiguration);

            var helloClient = RpcServiceProxyGenerator.CreateServiceProxy<IHelloService>(localActor, "server");
            var calcClient = RpcServiceProxyGenerator.CreateServiceProxy<ICalcService>(localActor, "server");

            localActor.RegisterRpcService(helloClient as RpcService);
            localActor.RegisterRpcService(calcClient as RpcService);

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
