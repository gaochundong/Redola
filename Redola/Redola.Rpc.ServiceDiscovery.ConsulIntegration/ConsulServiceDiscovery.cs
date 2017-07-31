using System;
using System.Collections.Generic;
using System.Linq;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.ConsulIntegration
{
    public class ConsulServiceDiscovery : IServiceDiscovery, IServiceDirectory
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

        public void RegisterService(ActorIdentity actor, Type serviceType)
        {
            _registry.RegisterService(actor, serviceType.FullName);
        }

        public void RegisterService(ActorIdentity actor, Type serviceType, IEnumerable<string> tags)
        {
            _registry.RegisterService(actor, serviceType.FullName, tags);
        }

        public void DeregisterService(ActorIdentity actor, Type serviceType)
        {
            _registry.DeregisterService(actor.Type, actor.Name, serviceType.FullName);
        }

        public void DeregisterService(string actorType, string actorName, Type serviceType)
        {
            _registry.DeregisterService(actorType, actorName, serviceType.FullName);
        }

        public IEnumerable<ActorIdentity> GetActors(Type serviceType)
        {
            return _registry.GetServices(serviceType.FullName).Select(s => s.ServiceActor);
        }
    }
}
