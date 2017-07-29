using System.Xml.Serialization;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    [XmlType(TypeName = "Entry")]
    public class XmlServiceRegistryEntry
    {
        [XmlElement(ElementName = "ServiceType")]
        public string ServiceType { get; set; }
        [XmlElement(ElementName = "ServiceActor")]
        public ServiceActor ServiceActor { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.ServiceType);
        }
    }
}
