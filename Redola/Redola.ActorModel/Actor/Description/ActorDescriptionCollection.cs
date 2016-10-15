using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [Serializable]
    [XmlRoot]
    public class ActorDescriptionCollection
    {
        public ActorDescriptionCollection()
        {
            Items = new List<ActorDescription>();
        }

        [XmlArray]
        public List<ActorDescription> Items { get; set; }
    }
}
