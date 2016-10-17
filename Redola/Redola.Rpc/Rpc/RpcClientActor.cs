using System;

namespace Redola.Rpc
{
    public class RpcClientActor
    {
        private static readonly IActorMessageEncoder _encoder = new ActorMessageEncoder(new ProtocolBuffersMessageEncoder());
        private static readonly IActorMessageDecoder _decoder = new ActorMessageDecoder(new ProtocolBuffersMessageDecoder());

        private BlockingRouteActor _localActor = null;

        public RpcClientActor()
        {
        }

        public BlockingRouteActor Actor { get { return _localActor; } }

        public void Bootup()
        {
            if (_localActor != null)
                throw new InvalidOperationException("Already bootup.");

            var configruation = new RpcActorConfiguration();
            configruation.Build();

            _localActor = new BlockingRouteActor(configruation, _encoder, _decoder);
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

        public void RegisterRpcClient(RpcClient client)
        {
            _localActor.RegisterMessageHandler(client);
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return _localActor.SendMessage<R, P>(remoteActorType, request, timeout);
        }
    }
}
