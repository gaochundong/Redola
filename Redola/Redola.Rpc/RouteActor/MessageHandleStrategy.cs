using System;

namespace Redola.ActorModel
{
    public sealed class MessageHandleStrategy
    {
        public MessageHandleStrategy()
        {
            this.AsyncWay = true;
        }

        public MessageHandleStrategy(Type messageType)
            : this()
        {
            this.MessageType = messageType;
        }

        public Type MessageType { get; set; }

        public bool AsyncWay { get; set; }

        public override string ToString()
        {
            return string.Format("MessageType[{0}], AsyncWay[{1}]",
                this.MessageType, this.AsyncWay);
        }
    }
}
