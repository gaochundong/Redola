using System;

namespace Redola.ActorModel
{
    public class ActorChannelSessionHandshakedEventArgs : EventArgs
    {
        public ActorChannelSessionHandshakedEventArgs(
            ActorChannelSession session,
            ActorDescription remoteActor)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");
            this.Session = session;
            this.RemoteActor = remoteActor;
        }

        public string SessionKey { get { return this.Session.SessionKey; } }
        public ActorChannelSession Session { get; private set; }
        public ActorDescription RemoteActor { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
