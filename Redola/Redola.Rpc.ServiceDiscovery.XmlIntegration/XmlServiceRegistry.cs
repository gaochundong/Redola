using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    [XmlType(TypeName = "Registry")]
    public class XmlServiceRegistry
    {
        [XmlArray(ElementName = "Entries")]
        public List<XmlServiceRegistryEntry> Entries { get; set; }
    }
}
