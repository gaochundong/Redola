namespace Redola.ActorModel.Framing
{
    public interface IActorControlFrameDataEncoder
    {
        byte[] EncodeFrameData<T>(T frameData);
    }
}
