using System.Collections.Generic;
using ProtoBuf;

namespace Redola.ActorModel
{
    [ProtoContract(SkipConstructor = false, UseProtoMembersOnly = true)]
    public class ActorLookupResponse
    {
        public ActorLookupResponse()
        {
        }

        [ProtoMember(10, OverwriteList = false)]
        public List<ActorDescription> Actors { get; set; }
    }
}
