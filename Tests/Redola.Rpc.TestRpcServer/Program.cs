using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;

namespace Redola.Rpc.TestRpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var actor = new RpcServiceActor();

            try
            {
                actor.Bootup();

                var orderService = new OrderService(actor);
                actor.RegisterRpcService(orderService);
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
                        break;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }

            actor.Shutdown();
        }
    }
}
