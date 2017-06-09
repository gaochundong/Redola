using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var localActor = new RpcActor();

            var helloClient = new HelloClient(localActor);
            var calcClient = new CalcClient(localActor);
            var orderClient = new OrderClient(localActor);

            localActor.RegisterRpcService(helloClient);
            localActor.RegisterRpcService(calcClient);
            localActor.RegisterRpcService(orderClient);

            localActor.Bootup();

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
                        localActor.Bootup();
                    }
                    else if (text == "hello")
                    {
                        Hello(log, helloClient);
                    }
                    else if (text == "hello10000")
                    {
                        Hello10000(log, helloClient);
                    }
                    else if (Regex.Match(text, @"hello(\d+)x(\d+)").Success)
                    {
                        var match = Regex.Match(text, @"hello(\d+)x(\d+)");
                        int totalCalls = int.Parse(match.Groups[1].Value);
                        int threadCount = int.Parse(match.Groups[2].Value);
                        Hello10000MultiThreading(log, helloClient, totalCalls, threadCount);
                    }
                    else if (Regex.Match(text, @"add(\d+)x(\d+)").Success)
                    {
                        var match = Regex.Match(text, @"add(\d+)x(\d+)");
                        int totalCalls = int.Parse(match.Groups[1].Value);
                        int threadCount = int.Parse(match.Groups[2].Value);
                        Add10000MultiThreading(log, calcClient, totalCalls, threadCount);
                    }
                    else
                    {
                        int times = 0;
                        if (int.TryParse(text, out times))
                        {
                            for (int i = 0; i < times; i++)
                            {
                                PlaceOrder(log, orderClient);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }

            localActor.Shutdown();
        }

        private static void Hello10000(ILog log, HelloClient helloClient)
        {
            log.DebugFormat("Hello10000, start ...");
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                var request = new ActorMessageEnvelope<Hello10000Request>()
                {
                    Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                };
                helloClient.SayHello10000(request);
            }
            watch.Stop();
            log.DebugFormat("Hello10000, end with cost {0} ms.", watch.ElapsedMilliseconds);
        }

        private static void Hello10000MultiThreading(ILog log, HelloClient helloClient, int totalCalls, int threadCount)
        {
            log.DebugFormat("Hello10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], start ...", totalCalls, threadCount);

            var taskList = new Task[threadCount];
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < threadCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    for (var j = 0; j < totalCalls / threadCount; j++)
                    {
                        var request = new ActorMessageEnvelope<Hello10000Request>()
                        {
                            Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                        };
                        helloClient.SayHello10000(request);
                    }
                },
                TaskCreationOptions.PreferFairness);
                taskList[i] = task;
            }
            Task.WaitAll(taskList);
            watch.Stop();

            log.DebugFormat("Hello10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], end with cost [{2}] ms."
                + "{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                totalCalls, threadCount, watch.ElapsedMilliseconds,
                Environment.NewLine, string.Format("   Concurrency level: {0} threads", threadCount),
                Environment.NewLine, string.Format("   Complete requests: {0}", totalCalls),
                Environment.NewLine, string.Format("Time taken for tests: {0} seconds", (decimal)watch.ElapsedMilliseconds / 1000m),
                Environment.NewLine, string.Format("    Time per request: {0:#####0.000} ms (avg)", (decimal)watch.ElapsedMilliseconds / (decimal)totalCalls),
                Environment.NewLine, string.Format(" Requests per second: {0} [#/sec] (avg)", (int)((decimal)totalCalls / ((decimal)watch.ElapsedMilliseconds / 1000m)))
                );
        }

        private static void Hello(ILog log, HelloClient helloClient)
        {
            var request = new ActorMessageEnvelope<HelloRequest>()
            {
                Message = new HelloRequest() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
            };

            log.DebugFormat("Hello, say hello to server with MessageID[{0}].",
                request.MessageID);

            var response = helloClient.SayHello(request);

            log.DebugFormat("Hello, receive hello response from server with MessageID[{0}] and CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
        }

        private static void Add10000MultiThreading(ILog log, CalcClient calcClient, int totalCalls, int threadCount)
        {
            log.DebugFormat("Add10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], start ...", totalCalls, threadCount);

            var taskList = new Task[threadCount];
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < threadCount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    for (var j = 0; j < totalCalls / threadCount; j++)
                    {
                        calcClient.Add(1, 2);
                    }
                },
                TaskCreationOptions.PreferFairness);
                taskList[i] = task;
            }
            Task.WaitAll(taskList);
            watch.Stop();

            log.DebugFormat("Add10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}], end with cost [{2}] ms."
                + "{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                totalCalls, threadCount, watch.ElapsedMilliseconds,
                Environment.NewLine, string.Format("   Concurrency level: {0} threads", threadCount),
                Environment.NewLine, string.Format("   Complete requests: {0}", totalCalls),
                Environment.NewLine, string.Format("Time taken for tests: {0} seconds", (decimal)watch.ElapsedMilliseconds / 1000m),
                Environment.NewLine, string.Format("    Time per request: {0:#####0.000} ms (avg)", (decimal)watch.ElapsedMilliseconds / (decimal)totalCalls),
                Environment.NewLine, string.Format(" Requests per second: {0} [#/sec] (avg)", (int)((decimal)totalCalls / ((decimal)watch.ElapsedMilliseconds / 1000m)))
                );
        }

        private static void PlaceOrder(ILog log, OrderClient orderClient)
        {
            var request = new ActorMessageEnvelope<PlaceOrderRequest>()
            {
                Message = new PlaceOrderRequest()
                {
                    Contract = new Order()
                    {
                        OrderID = Guid.NewGuid().ToString(),
                        ItemID = "Apple",
                        BuyCount = 100,
                    },
                },
            };

            log.DebugFormat("PlaceOrder, send place order request to server with MessageID[{0}].",
                request.MessageID);

            var response = orderClient.PlaceOrder(request);

            log.DebugFormat("PlaceOrder, receive place order response from server with MessageID[{0}] and CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
        }
    }
}
