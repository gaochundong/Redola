namespace Redola.Rpc
{
    public class MethodArgumentEncoder : IMethodArgumentEncoder
    {
        private IMessageEncoder _encoder;

        public MethodArgumentEncoder(IMessageEncoder encoder)
        {
            _encoder = encoder;
        }

        public byte[] Encode(object argument)
        {
            return _encoder.EncodeMessage(argument);
        }

        public byte[] Encode<T>(T argument)
        {
            return _encoder.EncodeMessage(argument);
        }
    }
}
