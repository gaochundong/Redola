using System;
using Logrila.Logging.NLogIntegration;

namespace Redola.Rpc.TestRpcClient
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            var configruation = new RpcActorConfiguration();
            configruation.Build();

            var actor = new RpcActor(configruation);
            actor.Bootup();

            while (true)
            {
                try
                {
                    string text = Console.ReadLine().ToLowerInvariant();
                    if (text == "quit" || text == "exit")
                    {
                        break;
                    }
                    else
                    {
                        int times = 0;
                        if (int.TryParse(text, out times))
                        {

                        }
                    }
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
