using System;

namespace Redola.Rpc
{
    public interface IRouteActorMessageHandler : IActorMessageHandler
    {
        Type GetAdmissibleMessageType(string messageType);
        MessageHandleStrategy GetAdmissibleMessageHandleStrategy(string messageType);
    }
}
