using System;
using System.Collections.Generic;
using System.Linq;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    public class LocalXmlFileServiceDirectory : IServiceDirectory
    {
        private LocalXmlFileServiceRegistry _registry;

        public LocalXmlFileServiceDirectory(LocalXmlFileServiceRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
            _registry = registry;
        }

        public void RegisterService(ActorIdentity actor, Type serviceType)
        {
        }

        public void RegisterService(ActorIdentity actor, Type serviceType, IEnumerable<string> tags)
        {
        }

        public void DeregisterService(ActorIdentity actor, Type serviceType)
        {
        }

        public void DeregisterService(string actorType, string actorName, Type serviceType)
        {
        }

        public IEnumerable<ActorIdentity> GetActors(Type serviceType)
        {
            return _registry.GetEntries()
                .Where(s => s.ServiceType == serviceType.FullName)
                .Select(s => s.ServiceActor);
        }
    }
}
