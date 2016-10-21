using System;

namespace Redola.ActorModel
{
    public class ActorChannelSessionDataReceivedEventArgs : EventArgs
    {
        public ActorChannelSessionDataReceivedEventArgs(
            ActorChannelSession session,
            ActorIdentity remoteActor,
            byte[] data)
            : this(session, remoteActor, data, 0, data.Length)
        {
        }

        public ActorChannelSessionDataReceivedEventArgs(
            ActorChannelSession session,
            ActorIdentity remoteActor,
            byte[] data, int dataOffset, int dataLength)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");
            this.Session = session;
            this.RemoteActor = remoteActor;

            this.Data = data;
            this.DataOffset = dataOffset;
            this.DataLength = dataLength;
        }

        public string SessionKey { get { return this.Session.SessionKey; } }
        public ActorChannelSession Session { get; private set; }
        public ActorIdentity RemoteActor { get; private set; }

        public byte[] Data { get; private set; }
        public int DataOffset { get; private set; }
        public int DataLength { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
