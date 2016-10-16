using System;

namespace Redola.ActorModel
{
    public class ActorTransportSessionDataReceivedEventArgs : EventArgs
    {
        public ActorTransportSessionDataReceivedEventArgs(ActorTransportSession session, byte[] data)
            : this(session, data, 0, data.Length)
        {
        }

        public ActorTransportSessionDataReceivedEventArgs(ActorTransportSession session, byte[] data, int dataOffset, int dataLength)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            Session = session;

            Data = data;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public string SessionKey { get { return this.Session.SessionKey; } }
        public ActorTransportSession Session { get; private set; }

        public byte[] Data { get; private set; }
        public int DataOffset { get; private set; }
        public int DataLength { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
