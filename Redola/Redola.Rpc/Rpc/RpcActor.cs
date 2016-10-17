using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcActor : BlockingRouteActor
    {
        private static readonly IActorMessageEncoder _encoder = new ActorMessageEncoder(new ProtocolBuffersMessageEncoder());
        private static readonly IActorMessageDecoder _decoder = new ActorMessageDecoder(new ProtocolBuffersMessageDecoder());

        public RpcActor(ActorConfiguration configuration)
            : base(configuration, _encoder, _decoder)
        {
        }
    }
}
