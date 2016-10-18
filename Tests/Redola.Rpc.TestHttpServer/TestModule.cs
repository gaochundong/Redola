using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Happer.Http;

namespace Redola.Rpc.TestHttpServer
{
    public class TestModule : Module
    {
        private HelloClient _helloService;

        public TestModule(HelloClient helloService)
        {
            _helloService = helloService;
        }
    }
}
