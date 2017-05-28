using System;

namespace Redola.ActorModel
{
    public class ActorChannelSessionHandshakedEventArgs : EventArgs
    {
        public ActorChannelSessionHandshakedEventArgs(
            ActorSessionChannel session,
            ActorIdentity remoteActor)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");
            this.Session = session;
            this.RemoteActor = remoteActor;
        }

        public string SessionKey { get { return this.Session.SessionKey; } }
        public ActorSessionChannel Session { get; private set; }
        public ActorIdentity RemoteActor { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
