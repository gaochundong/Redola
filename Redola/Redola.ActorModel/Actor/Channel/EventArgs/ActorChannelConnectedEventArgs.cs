using System;

namespace Redola.ActorModel
{
    public class ActorChannelConnectedEventArgs : EventArgs
    {
        public ActorChannelConnectedEventArgs(string channelIdentifier, ActorIdentity remoteActor)
        {
            if (string.IsNullOrEmpty(channelIdentifier))
                throw new ArgumentNullException("channelIdentifier");
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");

            this.ChannelIdentifier = channelIdentifier;
            this.RemoteActor = remoteActor;
        }

        public string ChannelIdentifier { get; private set; }
        public ActorIdentity RemoteActor { get; private set; }

        public override string ToString()
        {
            return string.Format("ChannelIdentifier[{0}], RemoteActor[{1}]", ChannelIdentifier, RemoteActor);
        }
    }
}
