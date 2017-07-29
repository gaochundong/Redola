using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [XmlType(TypeName = "Configuration")]
    public class XmlLocalXmlFileActorDirectoryConfiguration
    {
        [XmlArray(ElementName = "ActorDirectory")]
        public List<ActorIdentity> Directory { get; set; }
    }
}
