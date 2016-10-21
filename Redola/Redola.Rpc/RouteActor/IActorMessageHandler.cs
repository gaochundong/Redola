using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IActorMessageHandler
    {
        bool CanHandleMessage(ActorMessageEnvelope envelope);
        void HandleMessage(ActorIdentity remoteActor, ActorMessageEnvelope envelope);
    }
}
