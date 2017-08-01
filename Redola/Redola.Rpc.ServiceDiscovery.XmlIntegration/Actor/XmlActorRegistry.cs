using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    [XmlType(TypeName = "Registry")]
    public class XmlActorRegistry
    {
        [XmlArray(ElementName = "Entries")]
        public List<XmlActorRegistryEntry> Entries { get; set; }
    }
}
