using ProtoBuf;

namespace Redola.ActorModel
{
    [ProtoContract(SkipConstructor = false, UseProtoMembersOnly = true)]
    public class ActorLookupRequest
    {
        public ActorLookupRequest()
        {
        }
    }
}
