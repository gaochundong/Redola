using System;

namespace Redola.ActorModel
{
    public class ActorChannelDataReceivedEventArgs : EventArgs
    {
        public ActorChannelDataReceivedEventArgs(string channelIdentifier, ActorIdentity remoteActor, byte[] data)
            : this(channelIdentifier, remoteActor, data, 0, data.Length)
        {
        }

        public ActorChannelDataReceivedEventArgs(string channelIdentifier, ActorIdentity remoteActor, byte[] data, int dataOffset, int dataLength)
        {
            ChannelIdentifier = channelIdentifier;
            RemoteActor = remoteActor;

            Data = data;
            DataOffset = dataOffset;
            DataLength = dataLength;
        }

        public string ChannelIdentifier { get; private set; }
        public ActorIdentity RemoteActor { get; private set; }

        public byte[] Data { get; private set; }
        public int DataOffset { get; private set; }
        public int DataLength { get; private set; }

        public override string ToString()
        {
            return string.Format("ChannelIdentifier[{0}], RemoteActor[{1}]", ChannelIdentifier, RemoteActor);
        }
    }
}
