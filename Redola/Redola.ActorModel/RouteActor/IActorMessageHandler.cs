
namespace Redola.ActorModel
{
    public interface IActorMessageHandler
    {
        bool CanHandleMessage(MessageEnvelope envelope);
        void HandleMessage(ActorDescription remoteActor, MessageEnvelope envelope);
    }
}
