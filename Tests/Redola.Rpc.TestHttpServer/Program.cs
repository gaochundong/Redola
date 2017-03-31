using System;
using Happer;
using Happer.Hosting.Self;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;

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

            var actor = new RpcActor(localXmlFileActorConfiguration);

            var helloClient = new HelloClient(actor);
            var calcClient = new CalcClient(actor);

            actor.RegisterRpcService(helloClient);
            actor.RegisterRpcService(calcClient);

            actor.Bootup(localXmlFileActorDirectory);

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
