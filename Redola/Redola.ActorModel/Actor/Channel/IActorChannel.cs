using System;

namespace Redola.ActorModel
{
    public interface IActorChannel
    {
        bool Active { get; }

        void Open();
        void Close();

        event EventHandler<ActorConnectedEventArgs> Connected;
        event EventHandler<ActorDisconnectedEventArgs> Disconnected;
        event EventHandler<ActorDataReceivedEventArgs> DataReceived;

        void Send(string actorType, string actorName, byte[] data);
        void Send(string actorType, string actorName, byte[] data, int offset, int count);
        void SendAsync(string actorType, string actorName, byte[] data);
        void SendAsync(string actorType, string actorName, byte[] data, int offset, int count);

        void Send(string actorType, byte[] data);
        void Send(string actorType, byte[] data, int offset, int count);
        void SendAsync(string actorType, byte[] data);
        void SendAsync(string actorType, byte[] data, int offset, int count);
    }
}
