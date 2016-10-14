using System;

namespace Redola.ActorModel
{
    public class ActorDisconnectedEventArgs : EventArgs
    {
        public ActorDisconnectedEventArgs(string sessionKey, ActorDescription remoteActor)
        {
            if (string.IsNullOrEmpty(sessionKey))
                throw new ArgumentNullException("sessionKey");
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");

            this.SessionKey = sessionKey;
            this.RemoteActor = remoteActor;
        }

        public string SessionKey { get; private set; }
        public ActorDescription RemoteActor { get; private set; }

        public override string ToString()
        {
            return string.Format("SessionKey[{0}], RemoteActor[{1}]", SessionKey, RemoteActor);
        }
    }
}
