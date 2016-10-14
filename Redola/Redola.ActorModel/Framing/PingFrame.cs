using System;

namespace Redola.ActorModel
{
    public sealed class PingFrame : ControlFrame
    {
        public PingFrame(bool isMasked = true)
        {
            this.IsMasked = isMasked;
        }

        public PingFrame(string data, bool isMasked = true)
            : this(isMasked)
        {
            this.Data = data;
        }

        public string Data { get; private set; }
        public bool IsMasked { get; private set; }

        public override OpCode OpCode
        {
            get { return OpCode.Ping; }
        }

        public byte[] ToArray(IActorFrameBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            return builder.EncodeFrame(this);
        }
    }
}
