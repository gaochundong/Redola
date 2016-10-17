using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IActorMessageHandler
    {
        bool CanHandleMessage(ActorMessageEnvelope envelope);
        void HandleMessage(ActorDescription remoteActor, ActorMessageEnvelope envelope);
    }
}
