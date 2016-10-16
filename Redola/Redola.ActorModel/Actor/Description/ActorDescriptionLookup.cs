using System;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [Serializable]
    [XmlRoot]
    public class ActorDescriptionLookup
    {
        public ActorDescriptionLookup()
        {
        }

        [XmlElement]
        public string Type { get; set; }
    }
}
