namespace Redola.Rpc
{
    public class MethodArgumentEncoder : IMethodArgumentEncoder
    {
        private IObjectEncoder _encoder;

        public MethodArgumentEncoder(IObjectEncoder encoder)
        {
            _encoder = encoder;
        }

        public byte[] Encode(object argument)
        {
            return _encoder.Encode(argument);
        }

        public byte[] Encode<T>(T argument)
        {
            return _encoder.Encode(argument);
        }
    }
}
