using Happer.Http;

namespace Redola.Rpc.TestHttpServer
{
    public class TestModule : Module
    {
        private HelloClient _helloService;

        public TestModule(HelloClient helloService)
        {
            _helloService = helloService;

            Get["/hello"] = x =>
            {
                var response = _helloService.SayHello();
                return response == null ? string.Empty : response.Message.Text;
            };
        }
    }
}
