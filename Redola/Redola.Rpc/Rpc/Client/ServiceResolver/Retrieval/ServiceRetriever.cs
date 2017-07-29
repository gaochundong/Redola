using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class ServiceRetriever : IServiceRetriever
    {
        private IServiceDiscovery _discovery;

        public ServiceRetriever(IServiceDiscovery discovery)
        {
            if (discovery == null)
                throw new ArgumentNullException("discovery");
            _discovery = discovery;
        }

        public IEnumerable<ServiceActor> Retrieve(Type serviceType)
        {
            return _discovery.Discover(serviceType);
        }
    }
}
