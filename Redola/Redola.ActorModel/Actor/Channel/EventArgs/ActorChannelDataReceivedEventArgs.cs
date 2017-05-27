using System;

namespace Redola.ActorModel
{
    public class ActorChannelDataReceivedEventArgs : EventArgs
    {
        public ActorChannelDataReceivedEventArgs(string actorChannelIdentifier, ActorIdentity remoteActor, byte[] data)
            : this(actorChannelIdentifier, remoteActor, data, 0, data.Length)
        {
        }

        public ActorChannelDataReceivedEventArgs(string actorChannelIdentifier, ActorIdentity remoteActor, byte[] data, int dataOffset, int dataLength)
        {
            ActorChannelIdentifier = actorChannelIdentifier;
            RemoteActor = remoteActor;

            Data = data;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public string ActorChannelIdentifier { get; private set; }
        public ActorIdentity RemoteActor { get; private set; }

        public byte[] Data { get; private set; }
        public int DataOffset { get; private set; }
        public int DataLength { get; private set; }

        public override string ToString()
        {
            return string.Format("ActorChannelIdentifier[{0}], RemoteActor[{1}]", ActorChannelIdentifier, RemoteActor);
        }
    }
}
