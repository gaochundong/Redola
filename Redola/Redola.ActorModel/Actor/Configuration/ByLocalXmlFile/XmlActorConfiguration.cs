using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [XmlType(TypeName = "Configuration")]
    public class XmlActorConfiguration
    {
        [XmlElement(ElementName = "LocalActor")]
        public ActorIdentity LocalActor { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", this.LocalActor);
        }
    }
}
