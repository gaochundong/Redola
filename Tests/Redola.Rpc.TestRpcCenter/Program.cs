using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;

namespace Redola.Rpc.TestRpcCenter
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var master = new CenterActor();

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
                    log.Error(ex.Message, ex);
                }
            }

            master.Shutdown();
        }
    }
}
