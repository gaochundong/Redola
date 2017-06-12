using System;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.DynamicProxy.TestRpcServer
{
    internal class HelloService : IHelloService
    {
        public HelloResponse Hello(HelloRequest request)
        {
            return new HelloResponse() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") };
        }

        public Hello10000Response Hello10000(Hello10000Request request)
        {
            return new Hello10000Response() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") };
        }
    }
}
