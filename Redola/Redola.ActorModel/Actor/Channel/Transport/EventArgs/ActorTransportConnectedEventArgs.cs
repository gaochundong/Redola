using System;

namespace Redola.ActorModel
{
    public class ActorTransportConnectedEventArgs : EventArgs
    {
        public ActorTransportConnectedEventArgs(string sessionKey)
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
