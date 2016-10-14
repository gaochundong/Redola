namespace Redola.ActorModel
{
    public interface IActorFrameBuilder
    {
        byte[] EncodeFrame(PingFrame frame);
        byte[] EncodeFrame(PongFrame frame);
        byte[] EncodeFrame(BinaryFrame frame);

        bool TryDecodeFrameHeader(byte[] buffer, int offset, int count, out ActorFrameHeader frameHeader);
        void DecodePayload(byte[] buffer, int offset, ActorFrameHeader frameHeader, out byte[] payload, out int payloadOffset, out int payloadCount);
    }
}
