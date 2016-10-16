using System;

namespace Redola.ActorModel
{
    public class ActorTransportSessionDisconnectedEventArgs : EventArgs
    {
        public ActorTransportSessionDisconnectedEventArgs(ActorTransportSession session)
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
