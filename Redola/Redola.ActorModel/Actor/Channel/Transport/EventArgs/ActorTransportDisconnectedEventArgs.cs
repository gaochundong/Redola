using System;

namespace Redola.ActorModel
{
    public class ActorTransportDisconnectedEventArgs : EventArgs
    {
        public ActorTransportDisconnectedEventArgs(string sessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey))
                throw new ArgumentNullException("sessionKey");
            this.SessionKey = sessionKey;
        }

        public string SessionKey { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
