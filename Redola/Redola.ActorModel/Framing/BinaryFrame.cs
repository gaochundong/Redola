using System;

namespace Redola.ActorModel.Framing
{
    public sealed class BinaryFrame : DataFrame
    {
        public BinaryFrame(ArraySegment<byte> segment, bool isMasked = true)
        {
            ValidateArraySegment(segment, "segment");

            this.Data = segment.Array;
            this.Offset = segment.Offset;
            this.Count = segment.Count;
            this.IsMasked = isMasked;
        }

        public BinaryFrame(byte[] data, int offset, int count, bool isMasked = true)
        {
            ValidateBuffer(data, offset, count, "data");

            this.Data = data;
            this.Offset = offset;
            this.Count = count;
            this.IsMasked = isMasked;
        }

        public byte[] Data { get; private set; }
        public int Offset { get; private set; }
        public int Count { get; private set; }
        public bool IsMasked { get; private set; }

        public override OpCode OpCode
        {
            get { return OpCode.Binary; }
        }

        public byte[] ToArray(IActorFrameBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException("builder");
            return builder.EncodeFrame(this);
        }

        private static void ValidateBuffer(byte[] buffer, int offset, int count,
            string bufferParameterName = null,
            string offsetParameterName = null,
            string countParameterName = null)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(!string.IsNullOrEmpty(bufferParameterName) ? bufferParameterName : "buffer");
            }

            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(!string.IsNullOrEmpty(offsetParameterName) ? offsetParameterName : "offset");
            }

            if (count < 0 || count > (buffer.Length - offset))
            {
                throw new ArgumentOutOfRangeException(!string.IsNullOrEmpty(countParameterName) ? countParameterName : "count");
            }
        }

        private static void ValidateArraySegment<T>(ArraySegment<T> arraySegment, string arraySegmentParameterName = null)
        {
            if (arraySegment.Array == null)
            {
                throw new ArgumentNullException((!string.IsNullOrEmpty(arraySegmentParameterName) ? arraySegmentParameterName : "arraySegment") + ".Array");
            }

            if (arraySegment.Offset < 0 || arraySegment.Offset > arraySegment.Array.Length)
            {
                throw new ArgumentOutOfRangeException((!string.IsNullOrEmpty(arraySegmentParameterName) ? arraySegmentParameterName : "arraySegment") + ".Offset");
            }

            if (arraySegment.Count < 0 || arraySegment.Count > (arraySegment.Array.Length - arraySegment.Offset))
            {
                throw new ArgumentOutOfRangeException((!string.IsNullOrEmpty(arraySegmentParameterName) ? arraySegmentParameterName : "arraySegment") + ".Count");
            }
        }
    }
}
