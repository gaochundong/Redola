using System;

namespace Redola.ActorModel.Framing
{
    public sealed class ChangeFrame : ControlFrame
    {
        public ChangeFrame(bool isMasked = false)
        {
            this.IsMasked = isMasked;
        }

        public ChangeFrame(byte[] data, bool isMasked = false)
            : this(isMasked)
        {
            this.Data = data;
        }

        public byte[] Data { get; private set; }
        public bool IsMasked { get; private set; }

        public override OpCode OpCode
        {
            get { return OpCode.Change; }
        }

        public byte[] ToArray(IActorFrameBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            return builder.EncodeFrame(this);
        }
    }
}
