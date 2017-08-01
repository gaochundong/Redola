using System;
using System.Collections.Generic;
using System.Linq;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    public class LocalXmlFileServiceDiscovery : IServiceDiscovery
    {
        private LocalXmlFileServiceRegistry _registry;

        public LocalXmlFileServiceDiscovery(LocalXmlFileServiceRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
            _registry = registry;
        }

        public IEnumerable<ServiceActor> Discover(Type serviceType)
        {
            return _registry.GetEntries()
                .Where(s => s.ServiceType == serviceType.FullName)
                .Select(s => new ServiceActor(s.ServiceActor.Type, s.ServiceActor.Name));
        }
    }
}
