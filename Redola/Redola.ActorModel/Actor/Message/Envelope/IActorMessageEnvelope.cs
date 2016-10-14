using System;

namespace Redola.ActorModel
{
    public interface IActorMessageEnvelope
    {
        string MessageID { get; set; }
        DateTime MessageTime { get; set; }

        string MessageType { get; set; }
        byte[] MessageData { get; set; }
    }
}
