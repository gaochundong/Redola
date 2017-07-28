namespace Redola.Rpc
{
    public class ActorMessageEncoder : IActorMessageEncoder
    {
        private IMessageEncoder _encoder;

        public ActorMessageEncoder(IMessageEncoder encoder)
        {
            _encoder = encoder;
        }

        public byte[] EncodeMessage(object message)
        {
            return _encoder.EncodeMessage(message);
        }

        public byte[] EncodeMessage<T>(T message)
        {
            return _encoder.EncodeMessage(message);
        }

        public byte[] EncodeMessageEnvelope<T>(T message)
        {
            var envelope = new ActorMessageEnvelope()
            {
                MessageType = typeof(T).Name,
                MessageData = EncodeMessage(message),
            };
            return _encoder.EncodeMessage(envelope);
        }
    }
}
