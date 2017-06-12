using System;
using Happer.Http;
using Redola.Rpc.TestContracts;

namespace Redola.Rpc.TestHttpServer
{
    internal class TestModule : Module
    {
        private IHelloService _helloService;
        private ICalcService _calcService;

        public TestModule(IHelloService helloService, ICalcService calcService)
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
                var response = _calcService.Add(new AddRequest() { X = 1, Y = 2 });
                return string.Format("Result = {0}, Time = {1}", response.Result.ToString(), DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss.fffffff"));
            });
        }
    }
}
