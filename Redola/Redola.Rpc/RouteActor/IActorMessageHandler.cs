namespace Redola.Rpc
{
    public interface IActorMessageHandler
    {
        bool CanHandleMessage(ActorMessageEnvelope envelope);
        void HandleMessage(ActorSender sender, ActorMessageEnvelope envelope);
    }
}
