using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.ConsulIntegration
{
    public class ConsulServiceRegistryEntry
    {
        public string ServiceType { get; set; }
        public ActorIdentity ServiceActor { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.ServiceType);
        }
    }
}
