using System.Collections.Generic;
using System.Xml.Serialization;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.XmlIntegration
{
    [XmlType(TypeName = "Registry")]
    public class XmlActorRegistry
    {
        [XmlArray(ElementName = "Entries")]
        public List<ActorIdentity> Entries { get; set; }
    }
}
