using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RpcActor : BlockingRouteActor
    {
        public RpcActor(
            ActorConfiguration configuration,
            IActorMessageEncoder encoder,
            IActorMessageDecoder decoder)
            : base(configuration, encoder, decoder)
        {
        }
    }
}
