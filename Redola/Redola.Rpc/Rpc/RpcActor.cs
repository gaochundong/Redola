namespace Redola.Rpc
{
    public class RpcActor : BlockingRouteActor
    {
        private static readonly IActorMessageEncoder _encoder = new ActorMessageEncoder(new ProtocolBuffersMessageEncoder());
        private static readonly IActorMessageDecoder _decoder = new ActorMessageDecoder(new ProtocolBuffersMessageDecoder());

        public RpcActor(RpcActorConfiguration configuration)
            : base(configuration, _encoder, _decoder)
        {
        }

        public void RegisterRpcService(RpcService service)
        {
            this.RegisterMessageHandler(service);
        }
    }
}
