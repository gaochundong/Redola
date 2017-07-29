using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.ActorModel;

namespace Redola.Rpc.TestRegisterCenter
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
            var localActorConfiguration = AppConfigActorConfiguration.Load();
            var localActor = new CenterActor(localActorConfiguration);

            var directoryConfiguration = AppConfigCenterActorDirectoryConfiguration.Load();
            var directory = new CenterActorDirectory(directoryConfiguration);
            localActor.Bootup(directory);

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
                    _log.Error(ex.Message, ex);
                }
            }

            localActor.Shutdown();
        }
    }
}
