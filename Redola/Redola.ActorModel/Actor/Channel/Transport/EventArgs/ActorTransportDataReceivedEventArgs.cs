using System;

namespace Redola.ActorModel
{
    public class ActorTransportDataReceivedEventArgs : EventArgs
    {
        public ActorTransportDataReceivedEventArgs(string sessionKey, byte[] data)
            : this(sessionKey, data, 0, data.Length)
        {
        }

        public ActorTransportDataReceivedEventArgs(string sessionKey, byte[] data, int dataOffset, int dataLength)
        {
            SessionKey = sessionKey;
            Data = data;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public string SessionKey { get; private set; }

        public byte[] Data { get; private set; }
        public int DataOffset { get; private set; }
        public int DataLength { get; private set; }

        public override string ToString()
        {
            return SessionKey;
        }
    }
}
