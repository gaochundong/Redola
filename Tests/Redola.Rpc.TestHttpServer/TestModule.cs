using System;
using Happer.Http;

namespace Redola.Rpc.TestHttpServer
{
    public class TestModule : Module
    {
        private HelloClient _helloService;

        public TestModule(HelloClient helloService)
        {
            _helloService = helloService;

            Get["/empty"] = x =>
            {
                return string.Empty;
            };
            Get["/time"] = x =>
            {
                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            };
            Get["/hello"] = x =>
            {
                var response = _helloService.SayHello();
                return response == null ? string.Empty : response.Message.Text;
            };
        }
    }
}
