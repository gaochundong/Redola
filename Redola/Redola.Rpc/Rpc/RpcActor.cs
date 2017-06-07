using System;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcActor
    {
        public static readonly IActorMessageEncoder DefaultActorMessageEncoder = new ActorMessageEncoder(new ProtocolBuffersMessageEncoder());
        public static readonly IActorMessageDecoder DefaultActorMessageDecoder = new ActorMessageDecoder(new ProtocolBuffersMessageDecoder());

        private BlockingRouteActor _localActor = null;

        public RpcActor()
            : this(AppConfigActorConfiguration.Load())
        {
        }

        public RpcActor(ActorConfiguration configuration)
            : this(configuration, DefaultActorMessageEncoder, DefaultActorMessageDecoder)
        {
        }

        public RpcActor(ActorConfiguration configuration, IActorMessageEncoder encoder, IActorMessageDecoder decoder)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            if (encoder == null)
                throw new ArgumentNullException("encoder");
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            _localActor = new BlockingRouteActor(configuration, encoder, decoder);
        }

        public BlockingRouteActor Actor { get { return _localActor; } }
        public IActorMessageEncoder Encoder { get { return _localActor.Encoder; } }
        public IActorMessageDecoder Decoder { get { return _localActor.Decoder; } }

        public void Bootup()
        {
            _localActor.Bootup();
        }

        public void Bootup(IActorDirectory directory)
        {
            _localActor.Bootup(directory);
        }

        public void Shutdown()
        {
            _localActor.Shutdown();
        }

        public void RegisterRpcService(RpcService service)
        {
            _localActor.RegisterMessageHandler(service);
        }

        #region Send

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request)
        {
            return Send<R, P>(remoteActorType, request, TimeSpan.FromSeconds(30));
        }

        public ActorMessageEnvelope<P> Send<R, P>(string remoteActorType, ActorMessageEnvelope<R> request, TimeSpan timeout)
        {
            return _localActor.SendMessage<R, P>(remoteActorType, request, timeout);
        }

        public void Send<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            _localActor.Send(remoteActor, message);
        }

        public void BeginSend<T>(ActorIdentity remoteActor, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginSend(remoteActor, message);
        }

        public void Send<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            _localActor.Send(remoteActorType, remoteActorName, message);
        }

        public void BeginSend<T>(string remoteActorType, string remoteActorName, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginSend(remoteActorType, remoteActorName, message);
        }

        public void Send<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.Send(remoteActorType, message);
        }

        public void BeginSend<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginSend(remoteActorType, message);
        }

        #endregion

        #region Broadcast

        public void Broadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.Broadcast(remoteActorType, message);
        }

        public void BeginBroadcast<T>(string remoteActorType, ActorMessageEnvelope<T> message)
        {
            _localActor.BeginBroadcast(remoteActorType, message);
        }

        #endregion
    }
}
