using System;

namespace Redola.Rpc
{
    public class RpcMethodFixture
    {
        public RpcMethodFixture(
            IMethodLocatorExtractor extractor,
            IMethodArgumentEncoder argumentEncoder,
            IMethodArgumentDecoder argumentDecoder)
        {
            if (extractor == null)
                throw new ArgumentNullException("extractor");
            if (argumentEncoder == null)
                throw new ArgumentNullException("argumentEncoder");
            if (argumentDecoder == null)
                throw new ArgumentNullException("argumentDecoder");

            this.Extractor = extractor;
            this.ArgumentEncoder = argumentEncoder;
            this.ArgumentDecoder = argumentDecoder;
        }

        public IMethodLocatorExtractor Extractor { get; private set; }
        public IMethodArgumentEncoder ArgumentEncoder { get; private set; }
        public IMethodArgumentDecoder ArgumentDecoder { get; private set; }
    }
}
