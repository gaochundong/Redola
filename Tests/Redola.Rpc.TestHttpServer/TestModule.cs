using System;
using Happer.Http;

namespace Redola.Rpc.TestHttpServer
{
    public class TestModule : Module
    {
        private HelloClient _helloService;
        private CalcClient _calcService;

        public TestModule(HelloClient helloService, CalcClient calcService)
        {
            _helloService = helloService;
            _calcService = calcService;

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
            Get["/hello10000"] = x =>
            {
                var response = _helloService.SayHello10000();
                return response == null ? string.Empty : response.Message.Text;
            };
            Get["/add10000"] = x =>
            {
                var result = _calcService.Add(1, 2);
                return result.ToString();
            };
        }
    }
}
