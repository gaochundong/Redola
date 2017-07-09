using System;
using Logrila.Logging;
using Logrila.Logging.NLogIntegration;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.DynamicProxy.TestRpcServer
{
    class Program
    {
        static void Main(string[] args)
        {
            NLogLogger.Use();

            ILog log = Logger.Get<Program>();

            var localActor = new RpcActor();

            localActor.Register<IHelloService>(new HelloService());
            localActor.Register<ICalcService>(new CalcService());
            localActor.Register<IOrderService>(new OrderService());

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
                    else
                    {
                        log.WarnFormat("Cannot parse the operation for input [{0}].", text);
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex);
                }
            }

            localActor.Shutdown();
        }
    }
}
