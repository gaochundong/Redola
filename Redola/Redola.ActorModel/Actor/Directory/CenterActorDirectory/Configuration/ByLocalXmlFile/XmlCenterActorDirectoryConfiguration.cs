using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [XmlType(TypeName = "Configuration")]
    public class XmlCenterActorDirectoryConfiguration
    {
        [XmlElement(ElementName = "CenterActor")]
        public ActorIdentity CenterActor { get; set; }

        [XmlArray(ElementName = "ActorDirectory")]
        public List<ActorIdentity> Directory { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.CenterActor);
        }
    }
}
