using System;

namespace Redola.Rpc
{
    public sealed class MessageHandleStrategy
    {
        public MessageHandleStrategy()
        {
            this.IsHandledInSeparateThread = true;
        }

        public MessageHandleStrategy(Type messageType)
            : this()
        {
            this.MessageType = messageType;
        }

        public Type MessageType { get; set; }

        public bool IsHandledInSeparateThread { get; set; }

        public override string ToString()
        {
            return string.Format("MessageType[{0}], IsHandledInSeparateThread[{1}]",
                this.MessageType, this.IsHandledInSeparateThread);
        }
    }
}
