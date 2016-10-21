using System;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [Serializable]
    [XmlRoot]
    public class ActorIdentityLookup
    {
        public ActorIdentityLookup()
        {
        }

        [XmlElement]
        public string Type { get; set; }
    }
}
