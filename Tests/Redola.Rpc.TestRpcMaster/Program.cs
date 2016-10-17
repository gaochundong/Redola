using System;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;

namespace Redola.Rpc.TestRpcMaster
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            var configruation = new RpcActorConfiguration();
            configruation.Build();

            var master = new ActorMaster(configruation);
            master.Bootup();

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

            master.Shutdown();
        }
    }
}
