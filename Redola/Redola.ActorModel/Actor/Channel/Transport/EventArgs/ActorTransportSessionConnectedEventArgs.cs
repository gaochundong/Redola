using System;

namespace Redola.ActorModel
{
    public class ActorTransportSessionConnectedEventArgs : EventArgs
    {
        public ActorTransportSessionConnectedEventArgs(ActorTransportSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            this.Session = session;
        }

        public string SessionKey { get { return this.Session.SessionKey; } }
        public ActorTransportSession Session { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
