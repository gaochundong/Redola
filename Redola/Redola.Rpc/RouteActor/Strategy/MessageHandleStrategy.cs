using System;

namespace Redola.Rpc
{
    public sealed class MessageHandleStrategy
    {
        public MessageHandleStrategy()
        {
            this.IsAsyncPattern = true;
            this.IsOneWay = false;
        }

        public MessageHandleStrategy(Type messageType)
            : this()
        {
            this.MessageType = messageType;
        }

        public Type MessageType { get; set; }

        public bool IsAsyncPattern { get; set; }

        public bool IsOneWay { get; set; }

        public override string ToString()
        {
            return string.Format("MessageType[{0}], IsOneWay[{1}], IsAsyncPattern[{2}]",
                this.MessageType, this.IsOneWay, this.IsAsyncPattern);
        }
    }
}
