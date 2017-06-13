using System;

namespace Redola.ActorModel
{
    public interface IActorChannel
    {
        string Identifier { get; }
        ActorIdentity LocalActor { get; }
        ActorIdentity RemoteActor { get; }

        bool Active { get; }
        void Open();
        void Close();

        event EventHandler<ActorChannelConnectedEventArgs> ChannelConnected;
        event EventHandler<ActorChannelDisconnectedEventArgs> ChannelDisconnected;
        event EventHandler<ActorChannelDataReceivedEventArgs> ChannelDataReceived;

        void Send(string identifier, byte[] data);
        void Send(string identifier, byte[] data, int offset, int count);
        void BeginSend(string identifier, byte[] data);
        void BeginSend(string identifier, byte[] data, int offset, int count);

        IAsyncResult BeginSend(string identifier, byte[] data, AsyncCallback callback, object state);
        IAsyncResult BeginSend(string identifier, byte[] data, int offset, int count, AsyncCallback callback, object state);
        void EndSend(string identifier, IAsyncResult asyncResult);
    }
}
