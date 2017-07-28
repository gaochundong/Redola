using System;
using System.Collections.Generic;
using System.Linq;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    public class LocalXmlFileServiceDiscovery : IServiceDiscovery
    {
        private LocalXmlFileServiceRegistry _registry;

        public LocalXmlFileServiceDiscovery(string localXmlFilePath)
        {
            _registry = LocalXmlFileServiceRegistry.Load(localXmlFilePath);
        }

        public IEnumerable<ActorIdentity> Discover(Type serviceType)
        {
            return _registry.Entries.Where(e => e.ServiceType == serviceType.FullName).Select(e => e.ServiceActor);
        }
    }
}
