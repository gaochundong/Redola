using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Redola.ActorModel
{
    [Serializable]
    [XmlRoot]
    public class ActorIdentityCollection
    {
        public ActorIdentityCollection()
        {
            Items = new List<ActorIdentity>();
        }

        [XmlArray]
        public List<ActorIdentity> Items { get; set; }
    }
}
