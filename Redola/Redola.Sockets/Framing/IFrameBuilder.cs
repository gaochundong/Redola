namespace Redola.Sockets
{
    public interface IFrameBuilder
    {
        IFrameEncoder Encoder { get; }
        IFrameDecoder Decoder { get; }
    }
}
