using System;

namespace Redola.ActorModel.Framing
{
    public sealed class HelloFrame : ControlFrame
    {
        public HelloFrame(bool isMasked = false)
        {
            this.IsMasked = isMasked;
        }

        public HelloFrame(byte[] data, bool isMasked = false)
            : this(isMasked)
        {
            this.Data = data;
        }

        public byte[] Data { get; private set; }
        public bool IsMasked { get; private set; }

        public override OpCode OpCode
        {
            get { return OpCode.Hello; }
        }

        public byte[] ToArray(IActorFrameBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            return builder.EncodeFrame(this);
        }
    }
}
