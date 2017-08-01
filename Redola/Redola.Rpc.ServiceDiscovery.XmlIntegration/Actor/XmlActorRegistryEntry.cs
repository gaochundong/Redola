using System.Xml.Serialization;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    [XmlType(TypeName = "Entry")]
    public class XmlActorRegistryEntry
    {
        [XmlElement(ElementName = "ActorIdentity")]
        public ActorIdentity ActorIdentity { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.ActorIdentity);
        }
    }
}
