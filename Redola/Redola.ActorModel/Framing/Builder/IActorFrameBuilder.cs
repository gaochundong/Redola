namespace Redola.ActorModel.Framing
{
    public interface IActorFrameBuilder
    {
        IActorControlFrameDataEncoder ControlFrameDataEncoder { get; }
        IActorControlFrameDataDecoder ControlFrameDataDecoder { get; }

        byte[] EncodeFrame(HelloFrame frame);
        byte[] EncodeFrame(WelcomeFrame frame);
        byte[] EncodeFrame(PingFrame frame);
        byte[] EncodeFrame(PongFrame frame);
        byte[] EncodeFrame(WhereFrame frame);
        byte[] EncodeFrame(HereFrame frame);
        byte[] EncodeFrame(BinaryFrame frame);
        byte[] EncodeFrame(ChangeFrame frame);

        bool TryDecodeFrameHeader(byte[] buffer, int offset, int count, out ActorFrameHeader frameHeader);
        void DecodePayload(byte[] buffer, int offset, ActorFrameHeader frameHeader, out byte[] payload, out int payloadOffset, out int payloadCount);
    }
}
