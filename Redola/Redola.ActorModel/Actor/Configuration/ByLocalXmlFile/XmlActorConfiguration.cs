using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [XmlType(TypeName = "ActorConfiguration")]
    public class XmlActorConfiguration
    {
        [XmlElement(ElementName = "CenterActor")]
        public ActorIdentity CenterActor { get; set; }
        [XmlElement(ElementName = "LocalActor")]
        public ActorIdentity LocalActor { get; set; }

        [XmlArray(ElementName = "ActorDirectory")]
        public List<ActorIdentity> Directory { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.LocalActor);
        }
    }
}
