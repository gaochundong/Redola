using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.ConsulIntegration
{
    public class ConsulActorRegistryEntry
    {
        public string ActorKey { get; set; }
        public ActorIdentity ActorIdentity { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.ActorKey);
        }
    }
}
