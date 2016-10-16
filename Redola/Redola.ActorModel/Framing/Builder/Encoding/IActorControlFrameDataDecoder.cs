namespace Redola.ActorModel.Framing
{
    public interface IActorControlFrameDataDecoder
    {
        T DecodeFrameData<T>(byte[] data, int offset, int count);
    }
}
