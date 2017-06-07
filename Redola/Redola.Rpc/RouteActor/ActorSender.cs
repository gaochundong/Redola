using Redola.ActorModel;

namespace Redola.Rpc
{
    public class ActorSender
    {
        public ActorSender()
        {
        }

        public ActorSender(ActorIdentity remoteActor, string channelIdentifier)
        {
            this.RemoteActor = remoteActor;
            this.ChannelIdentifier = channelIdentifier;
        }

        public ActorIdentity RemoteActor { get; set; }
        public string ChannelIdentifier { get; set; }

        public override string ToString()
        {
            return string.Format("RemoteActor[{0}], ChannelIdentifier[{1}]", RemoteActor, ChannelIdentifier);
        }
    }
}
