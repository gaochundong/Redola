using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public class RpcClient : RpcHandler
    {
        private IServiceProxyGenerator _proxyGenerator;

        public RpcClient(RpcActor localActor, IServiceProxyGenerator proxyGenerator)
            : base(localActor)
        {
            if (proxyGenerator == null)
                throw new ArgumentNullException("proxyGenerator");
            _proxyGenerator = proxyGenerator;
        }

        public RpcClient(RpcActor localActor, IRateLimiter rateLimiter, IServiceProxyGenerator proxyGenerator)
            : base(localActor, rateLimiter)
        {
            if (proxyGenerator == null)
                throw new ArgumentNullException("proxyGenerator");
            _proxyGenerator = proxyGenerator;
        }

        protected override IEnumerable<RpcMessageContract> RegisterRpcMessageContracts()
        {
            var messages = new List<RpcMessageContract>();

            messages.Add(new RequestResponseMessageContract(typeof(InvokeMethodRequest), typeof(InvokeMethodResponse)));

            return messages;
        }

        public T Resolve<T>()
        {
            return _proxyGenerator.CreateServiceProxy<T>(this);
        }
    }
}
