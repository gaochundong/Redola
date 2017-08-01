using System;
using System.Collections.Generic;
using System.Linq;

namespace Redola.Rpc.ServiceDiscovery.ConsulIntegration
{
    public class ConsulServiceDiscovery : IServiceDiscovery
    {
        private ConsulServiceRegistry _registry;

        public ConsulServiceDiscovery(ConsulServiceRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
            _registry = registry;
        }

        public IEnumerable<ServiceActor> Discover(Type serviceType)
        {
            return _registry.GetServices(serviceType.FullName).Select(s => new ServiceActor(s.ServiceActor.Type, s.ServiceActor.Name));
        }
    }
}
