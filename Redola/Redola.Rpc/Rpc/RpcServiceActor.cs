using System;

namespace Redola.Rpc
{
    public class RpcServiceActor
    {
        private static readonly IActorMessageEncoder _encoder = new ActorMessageEncoder(new ProtocolBuffersMessageEncoder());
        private static readonly IActorMessageDecoder _decoder = new ActorMessageDecoder(new ProtocolBuffersMessageDecoder());

        private RouteActor _localActor = null;

        public RpcServiceActor()
        {
        }

        public RouteActor Actor { get { return _localActor; } }

        public void Bootup()
        {
            if (_localActor != null)
                throw new InvalidOperationException("Already bootup.");

            var configruation = new RpcActorConfiguration();
            configruation.Build();

            _localActor = new RouteActor(configruation, _encoder, _decoder);
            _localActor.Bootup();
        }

        public void Shutdown()
        {
            if (_localActor != null)
            {
                _localActor.Shutdown();
                _localActor = null;
            }
        }

        public void RegisterRpcService(RpcService service)
        {
            _localActor.RegisterMessageHandler(service);
        }

        public void Send(string remoteActorType, string remoteActorName, byte[] data)
        {
            Send(remoteActorType, remoteActorName, data, 0, data.Length);
        }

        public void Send(string remoteActorType, string remoteActorName, byte[] data, int offset, int count)
        {
            _localActor.Send(remoteActorType, remoteActorName, data, offset, count);
        }

        public void BeginSend(string remoteActorType, string remoteActorName, byte[] data)
        {
            BeginSend(remoteActorType, remoteActorName, data, 0, data.Length);
        }

        public void BeginSend(string remoteActorType, string remoteActorName, byte[] data, int offset, int count)
        {
            _localActor.BeginSend(remoteActorType, remoteActorName, data, offset, count);
        }

        public IAsyncResult BeginSend(string remoteActorType, string remoteActorName, byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(remoteActorType, remoteActorName, data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(string remoteActorType, string remoteActorName, byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            return _localActor.BeginSend(remoteActorType, remoteActorName, data, offset, count, callback, state);
        }

        public void EndSend(string remoteActorType, string remoteActorName, IAsyncResult asyncResult)
        {
            _localActor.EndSend(remoteActorType, remoteActorName, asyncResult);
        }

        public void Send(string remoteActorType, byte[] data)
        {
            BeginSend(remoteActorType, data, 0, data.Length);
        }

        public void Send(string remoteActorType, byte[] data, int offset, int count)
        {
            _localActor.Send(remoteActorType, data, offset, count);
        }

        public void BeginSend(string remoteActorType, byte[] data)
        {
            BeginSend(remoteActorType, data, 0, data.Length);
        }

        public void BeginSend(string remoteActorType, byte[] data, int offset, int count)
        {
            _localActor.BeginSend(remoteActorType, data, offset, count);
        }

        public void Broadcast(string remoteActorType, byte[] data)
        {
            Broadcast(remoteActorType, data, 0, data.Length);
        }

        public void Broadcast(string remoteActorType, byte[] data, int offset, int count)
        {
            _localActor.Broadcast(remoteActorType, data, offset, count);
        }

        public void BeginBroadcast(string remoteActorType, byte[] data)
        {
            BeginBroadcast(remoteActorType, data, 0, data.Length);
        }

        public void BeginBroadcast(string remoteActorType, byte[] data, int offset, int count)
        {
            _localActor.BeginBroadcast(remoteActorType, data, offset, count);
        }
    }
}
