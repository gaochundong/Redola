using System;
using Happer.Http;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpServer
{
    internal class TestModule : Module
    {
        private HelloClient _helloService;
        private CalcClient _calcService;

        public TestModule(HelloClient helloService, CalcClient calcService)
        {
            _helloService = helloService;
            _calcService = calcService;

            Get("/empty", x =>
            {
                return string.Empty;
            });
            Get("/time", x =>
            {
                return DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff");
            });
            Get("/hello", x =>
            {
                var response = _helloService.Hello(new HelloRequest() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") });
                return response == null ? string.Empty : response.Text;
            });
            Get("/hello10000", x =>
            {
                var response = _helloService.Hello10000(new Hello10000Request() { Text = DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff") });
                return response == null ? string.Empty : response.Text;
            });
            Get("/add", x =>
            {
                var result = _calcService.Add(1, 2);
                return result.ToString();
            });
        }
    }
}
