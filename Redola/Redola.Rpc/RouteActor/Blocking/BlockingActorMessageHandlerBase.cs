using System;

namespace Redola.Rpc
{
    public abstract class BlockingActorMessageHandlerBase : RouteActorMessageHandlerBase, IBlockingActorMessageHandler
    {
        private BlockingRouteActor _localActor;

        public BlockingActorMessageHandlerBase(BlockingRouteActor localActor)
            : base(localActor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            _localActor = localActor;
        }

        public new BlockingRouteActor Actor { get { return _localActor; } }

        protected override void DoHandleMessage(ActorSender sender, ActorMessageEnvelope envelope)
        {
            if (GetAdmissibleMessageHandleStrategy(envelope.MessageType).IsOneWay)
            {
                base.DoHandleMessage(sender, envelope);
            }
            else
            {
                envelope.HandledBy(this.Actor, GetAdmissibleMessageType(envelope.MessageType), this.Actor.Decoder, sender,
                    (object o) =>
                    {
                        return o
                            .GetType()
                            .GetMethod("OnSyncMessage")
                            .MakeGenericMethod(GetAdmissibleMessageType(envelope.MessageType));
                    });
            }
        }
    }
}
