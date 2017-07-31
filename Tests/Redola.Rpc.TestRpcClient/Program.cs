using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;
using Redola.Rpc.DynamicProxy.CastleIntegration;
using Redola.Rpc.ServiceDiscovery.XmlIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
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

            var localXmlFileServiceRegistryPath = Environment.CurrentDirectory + @"\\XmlConfiguration\\ServiceRegistry.xml";
            var serviceRegistry = LocalXmlFileServiceRegistry.Load(localXmlFileServiceRegistryPath);
            var serviceDiscovery = new LocalXmlFileServiceDiscovery(serviceRegistry);
            var serviceRetriever = new ServiceRetriever(serviceDiscovery);
            var serviceResolver = new ServiceResolver(serviceRetriever);
            var proxyGenerator = new ServiceProxyGenerator(serviceResolver);

            var rpcClient = new RpcClient(localActor, proxyGenerator);

            var helloClient = rpcClient.Resolve<IHelloService>();
            var calcClient = rpcClient.Resolve<ICalcService>();
            var orderClient = rpcClient.Resolve<IOrderService>();

            localActor.Bootup(localXmlFileActorDirectory);

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant().Trim();
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
                    else if (Regex.Match(text, @"^hello(\d*)$").Success)
                    {
                        var match = Regex.Match(text, @"^hello(\d*)$");
                        int totalCalls = 0;
                        if (!int.TryParse(match.Groups[1].Value, out totalCalls))
                        {
                            totalCalls = 1;
                        }
                        for (int i = 0; i < totalCalls; i++)
                        {
                            Hello(helloClient);
                        }
                    }
                    else if (Regex.Match(text, @"^add(\d*)$").Success)
                    {
                        var match = Regex.Match(text, @"^add(\d*)$");
                        int totalCalls = 0;
                        if (!int.TryParse(match.Groups[1].Value, out totalCalls))
                        {
                            totalCalls = 1;
                        }
                        for (int i = 0; i < totalCalls; i++)
                        {
                            Add(calcClient);
                        }
                    }
                    else if (Regex.Match(text, @"^order(\d*)$").Success)
                    {
                        var match = Regex.Match(text, @"order(\d*)$");
                        int totalCalls = 0;
                        if (!int.TryParse(match.Groups[1].Value, out totalCalls))
                        {
                            totalCalls = 1;
                        }
                        for (int i = 0; i < totalCalls; i++)
                        {
                            PlaceOrder(orderClient);
                        }
                    }
                    else if (Regex.Match(text, @"^hello(\d+)x(\d+)$").Success)
                    {
                        var match = Regex.Match(text, @"^hello(\d+)x(\d+)$");
                        int totalCalls = int.Parse(match.Groups[1].Value);
                        int threadCount = int.Parse(match.Groups[2].Value);
                        Hello10000MultiThreading(helloClient, totalCalls, threadCount);
                    }
                    else if (Regex.Match(text, @"^add(\d+)x(\d+)$").Success)
                    {
                        var match = Regex.Match(text, @"^add(\d+)x(\d+)$");
                        int totalCalls = int.Parse(match.Groups[1].Value);
                        int threadCount = int.Parse(match.Groups[2].Value);
                        Add10000MultiThreading(calcClient, totalCalls, threadCount);
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

        private static void Hello(IHelloService helloClient)
        {
            var response = helloClient.Hello(new HelloRequest() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") });

            _log.DebugFormat("Hello, receive hello response from server with [{0}].", response.Text);
        }

        private static void Hello10000(IHelloService helloClient)
        {
            _log.DebugFormat("Hello10000, start ...");
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                helloClient.Hello10000(new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") });
            }
            watch.Stop();
            _log.DebugFormat("Hello10000, end with cost {0} ms.", watch.ElapsedMilliseconds);
        }

        private static void Hello10000MultiThreading(IHelloService helloClient, int totalCalls, int threadCount)
        {
            _log.DebugFormat("Hello10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], start ...", totalCalls, threadCount);

            var taskList = new Task[threadCount];
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < threadCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    for (var j = 0; j < totalCalls / threadCount; j++)
                    {
                        helloClient.Hello10000(new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") });
                    }
                },
                TaskCreationOptions.PreferFairness);
                taskList[i] = task;
            }
            Task.WaitAll(taskList);
            watch.Stop();

            _log.DebugFormat("Hello10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], end with cost [{2}] ms."
                + "{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                totalCalls, threadCount, watch.ElapsedMilliseconds,
                Environment.NewLine, string.Format("   Concurrency level: {0} threads", threadCount),
                Environment.NewLine, string.Format("   Complete requests: {0}", totalCalls),
                Environment.NewLine, string.Format("Time taken for tests: {0} seconds", (decimal)watch.ElapsedMilliseconds / 1000m),
                Environment.NewLine, string.Format("    Time per request: {0:#####0.000} ms (avg)", (decimal)watch.ElapsedMilliseconds / (decimal)totalCalls),
                Environment.NewLine, string.Format(" Requests per second: {0} [#/sec] (avg)", (int)((decimal)totalCalls / ((decimal)watch.ElapsedMilliseconds / 1000m)))
                );
        }

        private static void Add(ICalcService calcClient)
        {
            var response = calcClient.Add(new AddRequest() { X = 3, Y = 4 });

            _log.DebugFormat("Add, receive add response from server with [{0}].", response.Result);
        }

        private static void Add10000MultiThreading(ICalcService calcClient, int totalCalls, int threadCount)
        {
            _log.DebugFormat("Add10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], start ...", totalCalls, threadCount);

            var taskList = new Task[threadCount];
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < threadCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    for (var j = 0; j < totalCalls / threadCount; j++)
                    {
                        calcClient.Add(new AddRequest() { X = 1, Y = 2 });
                    }
                },
                TaskCreationOptions.PreferFairness);
                taskList[i] = task;
            }
            Task.WaitAll(taskList);
            watch.Stop();

            _log.DebugFormat("Add10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], end with cost [{2}] ms."
                + "{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                totalCalls, threadCount, watch.ElapsedMilliseconds,
                Environment.NewLine, string.Format("   Concurrency level: {0} threads", threadCount),
                Environment.NewLine, string.Format("   Complete requests: {0}", totalCalls),
                Environment.NewLine, string.Format("Time taken for tests: {0} seconds", (decimal)watch.ElapsedMilliseconds / 1000m),
                Environment.NewLine, string.Format("    Time per request: {0:#####0.000} ms (avg)", (decimal)watch.ElapsedMilliseconds / (decimal)totalCalls),
                Environment.NewLine, string.Format(" Requests per second: {0} [#/sec] (avg)", (int)((decimal)totalCalls / ((decimal)watch.ElapsedMilliseconds / 1000m)))
                );
        }

        private static void PlaceOrder(IOrderService orderClient)
        {
            var request = new PlaceOrderRequest()
            {
                Contract = new Order()
                {
                    OrderID = Guid.NewGuid().ToString(),
                    ItemID = "Apple",
                    BuyCount = 100,
                },
            };

            var response = orderClient.PlaceOrder(request);

            _log.DebugFormat("PlaceOrder, receive place order response from server with [{0}].", response.ErrorCode);
        }
    }
}
