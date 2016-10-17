using System;
using Logrila.Logging.NLogIntegration;

namespace Redola.Rpc.TestRpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            var configruation = new RpcActorConfiguration();
            configruation.Build();

            var actor = new RpcActor(configruation);

            var orderService = new OrderService(actor);
            actor.RegisterRpcService(orderService);

            actor.Bootup();

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                        break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            actor.Shutdown();
        }
    }
}
