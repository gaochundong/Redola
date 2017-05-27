using System;

namespace Redola.ActorModel
{
    public interface IActorChannel
    {
        string Identifier { get; }
        bool Active { get; }

        void Open();
        void Close();

        event EventHandler<ActorChannelConnectedEventArgs> ChannelConnected;
        event EventHandler<ActorChannelDisconnectedEventArgs> ChannelDisconnected;
        event EventHandler<ActorChannelDataReceivedEventArgs> ChannelDataReceived;

        void Send(string actorType, string actorName, byte[] data);
        void Send(string actorType, string actorName, byte[] data, int offset, int count);
        void BeginSend(string actorType, string actorName, byte[] data);
        void BeginSend(string actorType, string actorName, byte[] data, int offset, int count);

        void Send(string actorType, byte[] data);
        void Send(string actorType, byte[] data, int offset, int count);
        void BeginSend(string actorType, byte[] data);
        void BeginSend(string actorType, byte[] data, int offset, int count);

        IAsyncResult BeginSend(string actorType, string actorName, byte[] data, AsyncCallback callback, object state);
        IAsyncResult BeginSend(string actorType, string actorName, byte[] data, int offset, int count, AsyncCallback callback, object state);
        void EndSend(string actorType, string actorName, IAsyncResult asyncResult);
    }
}
