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

            try
            {
                actor.Bootup();

                var helloClient = new HelloClient(actor);
                var orderClient = new OrderClient(actor);

                actor.RegisterRpcService(helloClient);
                actor.RegisterRpcService(orderClient);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
            }

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
                        HelloWorld(log, actor);
                    }
                    else if (text == "hello10000")
                    {
                        HelloWorld10000(log, actor);
                    }
                    else if (text == "hello10000x2")
                    {
                        HelloWorld10000Half(log, actor);
                    }
                    else if (text == "hello10000x4")
                    {
                        HelloWorld10000Quarter(log, actor);
                    }
                    else
                    {
                        int times = 0;
                        if (int.TryParse(text, out times))
                        {
                            for (int i = 0; i < times; i++)
                            {
                                PlaceOrder(log, actor);
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

        private static void HelloWorld10000(ILog log, RpcActor actor)
        {
            log.DebugFormat("HelloWorld10000, start ...");
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 10000; i++)
            {
                var request = new ActorMessageEnvelope<Hello10000Request>()
                {
                    Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                };
                actor.Send<Hello10000Request, Hello10000Response>("server", request);
            }
            watch.Stop();
            log.DebugFormat("HelloWorld10000, end with cost {0} ms.", watch.ElapsedMilliseconds);
        }

        private static void HelloWorld10000Half(ILog log, RpcActor actor)
        {
            log.DebugFormat("HelloWorld10000Half, start ...");
            var taskList = new Task[2];
            var watch = Stopwatch.StartNew();
            var task1 = Task.Run(() =>
            {
                for (var i = 0; i < 5000; i++)
                {

                    var request = new ActorMessageEnvelope<Hello10000Request>()
                    {
                        Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                    };
                    actor.Send<Hello10000Request, Hello10000Response>("server", request);
                }
            });
            var task2 = Task.Run(() =>
            {
                for (var i = 0; i < 5000; i++)
                {

                    var request = new ActorMessageEnvelope<Hello10000Request>()
                    {
                        Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                    };
                    actor.Send<Hello10000Request, Hello10000Response>("server", request);
                }
            });
            taskList[0] = task1;
            taskList[1] = task2;
            Task.WaitAll(taskList);
            watch.Stop();
            log.DebugFormat("HelloWorld10000Half, end with cost {0} ms.", watch.ElapsedMilliseconds);
        }

        private static void HelloWorld10000Quarter(ILog log, RpcActor actor)
        {
            log.DebugFormat("HelloWorld10000Quarter, start ...");
            var taskList = new Task[4];
            var watch = Stopwatch.StartNew();
            var task1 = Task.Run(() =>
            {
                for (var i = 0; i < 2500; i++)
                {

                    var request = new ActorMessageEnvelope<Hello10000Request>()
                    {
                        Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                    };
                    actor.Send<Hello10000Request, Hello10000Response>("server", request);
                }
            });
            var task2 = Task.Run(() =>
            {
                for (var i = 0; i < 2500; i++)
                {

                    var request = new ActorMessageEnvelope<Hello10000Request>()
                    {
                        Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                    };
                    actor.Send<Hello10000Request, Hello10000Response>("server", request);
                }
            });
            var task3 = Task.Run(() =>
            {
                for (var i = 0; i < 2500; i++)
                {

                    var request = new ActorMessageEnvelope<Hello10000Request>()
                    {
                        Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                    };
                    actor.Send<Hello10000Request, Hello10000Response>("server", request);
                }
            });
            var task4 = Task.Run(() =>
            {
                for (var i = 0; i < 2500; i++)
                {

                    var request = new ActorMessageEnvelope<Hello10000Request>()
                    {
                        Message = new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
                    };
                    actor.Send<Hello10000Request, Hello10000Response>("server", request);
                }
            });
            taskList[0] = task1;
            taskList[1] = task2;
            taskList[2] = task2;
            taskList[3] = task3;
            Task.WaitAll(taskList);
            watch.Stop();
            log.DebugFormat("HelloWorld10000Quarter, end with cost {0} ms.", watch.ElapsedMilliseconds);
        }

        private static void HelloWorld(ILog log, RpcActor actor)
        {
            var request = new ActorMessageEnvelope<HelloRequest>()
            {
                Message = new HelloRequest() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") },
            };

            log.DebugFormat("HelloWorld, say hello to server with MessageID[{0}].",
                request.MessageID);

            var response = actor.Send<HelloRequest, HelloResponse>("server", request);

            log.DebugFormat("HelloWorld, receive hello response from server with MessageID[{0}] and CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
        }

        private static void PlaceOrder(ILog log, RpcActor actor)
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

            var response = actor.Send<PlaceOrderRequest, PlaceOrderResponse>("server", request);

            log.DebugFormat("PlaceOrder, receive place order response from server with MessageID[{0}] and CorrelationID[{1}].",
                response.MessageID, response.CorrelationID);
        }
    }
}
