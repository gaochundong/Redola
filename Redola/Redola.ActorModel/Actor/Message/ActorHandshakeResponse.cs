using ProtoBuf;

namespace Redola.ActorModel
{
    [ProtoContract(SkipConstructor = false, UseProtoMembersOnly = true)]
    public class ActorHandshakeResponse
    {
        public ActorHandshakeResponse()
        {
        }

        [ProtoMember(10)]
        public ActorDescription ActorDescription { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", ActorDescription);
        }
    }
}
