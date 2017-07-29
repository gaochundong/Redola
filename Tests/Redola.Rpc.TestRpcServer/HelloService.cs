using System;
using Logrila.Logging;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestRpcServer
{
    internal class HelloService : IHelloService
    {
        private ILog _log = Logger.Get<HelloService>();

        public HelloResponse Hello(HelloRequest request)
        {
            _log.DebugFormat("Hello, Text={0}", request.Text);
            return new HelloResponse() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") };
        }

        public Hello10000Response Hello10000(Hello10000Request request)
        {
            return new Hello10000Response() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") };
        }
    }
}
