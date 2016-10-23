using System;
using System.Diagnostics;
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

            var actor = new RpcActor();

            actor.Bootup();

            var helloClient = new HelloClient(actor);
            var calcClient = new CalcClient(actor);
            var orderClient = new OrderClient(actor);

            actor.RegisterRpcService(helloClient);
            actor.RegisterRpcService(calcClient);
            actor.RegisterRpcService(orderClient);

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                    {
                        break;
                    }
                    else if (text == "hello")
                    {
                        Hello(log, helloClient);
                    }
                    else if (text == "hello10000")
                    {
                        Hello10000(log, helloClient);
                    }
                    else if (text == "hello10000x1")
                    {
                        Hello10000MultiThreading(log, helloClient, 10000, 1);
                    }
                    else if (text == "hello10000x2")
                    {
                        Hello10000MultiThreading(log, helloClient, 10000, 2);
                    }
                    else if (text == "hello10000x4")
                    {
                        Hello10000MultiThreading(log, helloClient, 10000, 4);
                    }
                    else if (text == "hello10000x8")
                    {
                        Hello10000MultiThreading(log, helloClient, 10000, 8);
                    }
                    else if (text == "hello10000x16")
                    {
                        Hello10000MultiThreading(log, helloClient, 10000, 16);
                    }
                    else if (text == "hello10000x32")
                    {
                        Hello10000MultiThreading(log, helloClient, 10000, 32);
                    }
                    else if (text == "hello100000x1")
                    {
                        Hello10000MultiThreading(log, helloClient, 100000, 1);
                    }
                    else if (text == "hello100000x2")
                    {
                        Hello10000MultiThreading(log, helloClient, 100000, 2);
                    }
                    else if (text == "hello100000x4")
                    {
                        Hello10000MultiThreading(log, helloClient, 100000, 4);
                    }
                    else if (text == "hello100000x8")
                    {
                        Hello10000MultiThreading(log, helloClient, 100000, 8);
                    }
                    else if (text == "hello100000x16")
                    {
                        Hello10000MultiThreading(log, helloClient, 100000, 16);
                    }
                    else if (text == "hello100000x32")
                    {
                        Hello10000MultiThreading(log, helloClient, 100000, 32);
                    }
                    else if (text == "hello300000x1")
                    {
                        Hello10000MultiThreading(log, helloClient, 300000, 1);
                    }
                    else if (text == "hello300000x2")
                    {
                        Hello10000MultiThreading(log, helloClient, 300000, 2);
                    }
                    else if (text == "hello300000x4")
                    {
                        Hello10000MultiThreading(log, helloClient, 300000, 4);
                    }
                    else if (text == "hello300000x8")
                    {
                        Hello10000MultiThreading(log, helloClient, 300000, 8);
                    }
                    else if (text == "hello300000x16")
                    {
                        Hello10000MultiThreading(log, helloClient, 300000, 16);
                    }
                    else if (text == "hello300000x32")
                    {
                        Hello10000MultiThreading(log, helloClient, 300000, 32);
                    }
                    else if (text == "hello1500000x1")
                    {
                        Hello10000MultiThreading(log, helloClient, 1500000, 1);
                    }
                    else if (text == "hello1500000x2")
                    {
                        Hello10000MultiThreading(log, helloClient, 1500000, 2);
                    }
                    else if (text == "hello1500000x4")
                    {
                        Hello10000MultiThreading(log, helloClient, 1500000, 4);
                    }
                    else if (text == "hello1500000x8")
                    {
                        Hello10000MultiThreading(log, helloClient, 1500000, 8);
                    }
                    else if (text == "hello1500000x16")
                    {
                        Hello10000MultiThreading(log, helloClient, 1500000, 16);
                    }
                    else if (text == "hello1500000x32")
                    {
                        Hello10000MultiThreading(log, helloClient, 1500000, 32);
                    }
                    else if (text == "add10000x1")
                    {
                        Add10000MultiThreading(log, calcClient, 10000, 1);
                    }
                    else if (text == "add10000x2")
                    {
                        Add10000MultiThreading(log, calcClient, 10000, 2);
                    }
                    else if (text == "add10000x4")
                    {
                        Add10000MultiThreading(log, calcClient, 10000, 4);
                    }
                    else if (text == "add10000x8")
                    {
                        Add10000MultiThreading(log, calcClient, 10000, 8);
                    }
                    else if (text == "add10000x16")
                    {
                        Add10000MultiThreading(log, calcClient, 10000, 16);
                    }
                    else if (text == "add10000x32")
                    {
                        Add10000MultiThreading(log, calcClient, 10000, 32);
                    }
                    else if (text == "add100000x1")
                    {
                        Add10000MultiThreading(log, calcClient, 100000, 1);
                    }
                    else if (text == "add100000x2")
                    {
                        Add10000MultiThreading(log, calcClient, 100000, 2);
                    }
                    else if (text == "add100000x4")
                    {
                        Add10000MultiThreading(log, calcClient, 100000, 4);
                    }
                    else if (text == "add100000x8")
                    {
                        Add10000MultiThreading(log, calcClient, 100000, 8);
                    }
                    else if (text == "add100000x16")
                    {
                        Add10000MultiThreading(log, calcClient, 100000, 16);
                    }
                    else if (text == "add100000x32")
                    {
                        Add10000MultiThreading(log, calcClient, 100000, 32);
                    }
                    else if (text == "add300000x1")
                    {
                        Add10000MultiThreading(log, calcClient, 300000, 1);
                    }
                    else if (text == "add300000x2")
                    {
                        Add10000MultiThreading(log, calcClient, 300000, 2);
                    }
                    else if (text == "add300000x4")
                    {
                        Add10000MultiThreading(log, calcClient, 300000, 4);
                    }
                    else if (text == "add300000x8")
                    {
                        Add10000MultiThreading(log, calcClient, 300000, 8);
                    }
                    else if (text == "add300000x16")
                    {
                        Add10000MultiThreading(log, calcClient, 300000, 16);
                    }
                    else if (text == "add300000x32")
                    {
                        Add10000MultiThreading(log, calcClient, 300000, 32);
                    }
                    else if (text == "add1500000x1")
                    {
                        Add10000MultiThreading(log, calcClient, 1500000, 1);
                    }
                    else if (text == "add1500000x2")
                    {
                        Add10000MultiThreading(log, calcClient, 1500000, 2);
                    }
                    else if (text == "add1500000x4")
                    {
                        Add10000MultiThreading(log, calcClient, 1500000, 4);
                    }
                    else if (text == "add1500000x8")
                    {
                        Add10000MultiThreading(log, calcClient, 1500000, 8);
                    }
                    else if (text == "add1500000x16")
                    {
                        Add10000MultiThreading(log, calcClient, 1500000, 16);
                    }
                    else if (text == "add1500000x32")
                    {
                        Add10000MultiThreading(log, calcClient, 1500000, 32);
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

            actor.Shutdown();
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
                var task = Task.Run(() =>
                {
                    for (var j = 0; j < totalCalls / threadCount; j++)
                    {
                        var request = new ActorMessageEnvelope<Hello10000Request>()
                        {
                            Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                        };
                        helloClient.SayHello10000(request);
                    }
                });
                taskList[i] = task;
            }
            Task.WaitAll(taskList);
            watch.Stop();

            log.DebugFormat("Hello10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}] end with cost [{2}] ms."
                + "{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                totalCalls, threadCount, watch.ElapsedMilliseconds,
                Environment.NewLine, string.Format("   Concurrency level: {0} threads", threadCount),
                Environment.NewLine, string.Format("   Complete requests: {0}", totalCalls),
                Environment.NewLine, string.Format("Time taken for tests: {0} seconds", (decimal)watch.ElapsedMilliseconds / 1000m),
                Environment.NewLine, string.Format("    Time per request: {0:#####0.000} ms (mean)", (decimal)watch.ElapsedMilliseconds / (decimal)totalCalls),
                Environment.NewLine, string.Format(" Requests per second: {0} [#/sec] (mean)", (int)((decimal)totalCalls / ((decimal)watch.ElapsedMilliseconds / 1000m)))
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
                var task = Task.Run(() =>
                {
                    for (var j = 0; j < totalCalls / threadCount; j++)
                    {
                        calcClient.Add(1, 2);
                    }
                });
                taskList[i] = task;
            }
            Task.WaitAll(taskList);
            watch.Stop();

            log.DebugFormat("Add10000MultiThreading, TotalCalls[{0}], ThreadCount[{1}] end with cost [{2}] ms."
                + "{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}",
                totalCalls, threadCount, watch.ElapsedMilliseconds,
                Environment.NewLine, string.Format("   Concurrency level: {0} threads", threadCount),
                Environment.NewLine, string.Format("   Complete requests: {0}", totalCalls),
                Environment.NewLine, string.Format("Time taken for tests: {0} seconds", (decimal)watch.ElapsedMilliseconds / 1000m),
                Environment.NewLine, string.Format("    Time per request: {0:#####0.000} ms (mean)", (decimal)watch.ElapsedMilliseconds / (decimal)totalCalls),
                Environment.NewLine, string.Format(" Requests per second: {0} [#/sec] (mean)", (int)((decimal)totalCalls / ((decimal)watch.ElapsedMilliseconds / 1000m)))
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
