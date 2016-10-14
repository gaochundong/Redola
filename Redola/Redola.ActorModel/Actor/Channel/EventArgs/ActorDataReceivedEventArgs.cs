using System;

namespace Redola.ActorModel
{
    public class ActorDataReceivedEventArgs : EventArgs
    {
        public ActorDataReceivedEventArgs(string sessionKey, ActorDescription remoteActor, byte[] data)
            : this(sessionKey, remoteActor, data, 0, data.Length)
        {
        }

        public ActorDataReceivedEventArgs(string sessionKey, ActorDescription remoteActor, byte[] data, int dataOffset, int dataLength)
        {
            SessionKey = sessionKey;
            RemoteActor = remoteActor;

            Data = data;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public string SessionKey { get; private set; }
        public ActorDescription RemoteActor { get; private set; }

        public byte[] Data { get; private set; }
        public int DataOffset { get; private set; }
        public int DataLength { get; private set; }

        public override string ToString()
        {
            return string.Format("RemoteActor[{0}], SessionKey[{1}]", RemoteActor, SessionKey);
        }
    }
}
