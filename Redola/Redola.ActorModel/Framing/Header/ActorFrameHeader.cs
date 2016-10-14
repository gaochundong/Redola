namespace Redola.ActorModel.Framing
{
    public class ActorFrameFixedHeader
    {
        public bool IsRSV0 { get; set; }
        public bool IsRSV1 { get; set; }
        public bool IsRSV2 { get; set; }
        public bool IsRSV3 { get; set; }
        public OpCode OpCode { get; set; }
        public bool IsMasked { get; set; }

        public override string ToString()
        {
            return string.Format("IsRSV0[{0}], IsRSV1[{1}], IsRSV2[{2}], IsRSV3[{3}], OpCode[{4}], IsMasked[{5}]",
                IsRSV0, IsRSV1, IsRSV2, IsRSV3, OpCode, IsMasked);
        }
    }

    public class ActorFrameHeader : ActorFrameFixedHeader
    {
        public int PayloadLength { get; set; }
        public int MaskingKeyOffset { get; set; }
        public int Length { get; set; }

        public override string ToString()
        {
            return string.Format("IsRSV0[{0}], IsRSV1[{1}], IsRSV2[{2}], IsRSV3[{3}], OpCode[{4}], IsMasked[{5}], PayloadLength[{6}], MaskingKeyOffset[{7}], Length[{8}]",
                IsRSV0, IsRSV1, IsRSV2, IsRSV3, OpCode, IsMasked, PayloadLength, MaskingKeyOffset, Length);
        }
    }
}
