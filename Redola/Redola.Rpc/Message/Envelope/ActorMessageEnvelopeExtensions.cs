using System;
using System.Reflection;
using Logrila.Logging;

namespace Redola.Rpc
{
    public static class ActorMessageEnvelopeExtensions
    {
        private static ILog _log = Logger.Get<ActorMessageEnvelope>();

        public static ActorMessageEnvelope<T> Instantiate<T>(this ActorMessageEnvelope envelope, IActorMessageDecoder decoder)
        {
            var message = envelope.ConvertTo<T>();
            message.Message = decoder.DecodeMessage<T>(envelope.MessageData, 0, envelope.MessageData.Length);
            return message;
        }

        public static ActorMessageEnvelope Marshal<T>(this ActorMessageEnvelope<T> envelope, IActorMessageEncoder encoder)
        {
            var message = envelope.ConvertToNonGeneric();
            message.MessageData = encoder.EncodeMessage(envelope.Message);
            return message;
        }

        public static byte[] ToBytes(this ActorMessageEnvelope envelope, IActorMessageEncoder encoder)
        {
            return encoder.EncodeMessageEnvelope(envelope);
        }

        public static byte[] ToBytes<T>(this ActorMessageEnvelope<T> envelope, IActorMessageEncoder encoder)
        {
            return ToBytes(Marshal(envelope, encoder), encoder);
        }

        public static void HandledBy(this ActorMessageEnvelope envelope, object handlerFrom, Type messageType, IActorMessageDecoder decoder)
        {
            HandledBy(envelope, handlerFrom, messageType, decoder, (string s) => { return @"On" + s; });
        }

        public static void HandledBy(this ActorMessageEnvelope envelope, object handlerFrom, Type messageType, IActorMessageDecoder decoder, Func<string, string> getHandlerName)
        {
            HandledBy(envelope, handlerFrom, messageType, decoder,
                (object o) =>
                {
                    return o.GetType().GetMethod(getHandlerName(envelope.MessageType), BindingFlags.NonPublic | BindingFlags.Instance);
                });
        }

        public static void HandledBy(this ActorMessageEnvelope envelope, object handlerFrom, Type messageType, IActorMessageDecoder decoder, Func<object, MethodInfo> getHandlerMethod)
        {
            var instantiateMethod = typeof(ActorMessageEnvelopeExtensions)
                .GetMethod("Instantiate", new Type[] { typeof(ActorMessageEnvelope), typeof(IActorMessageDecoder) })
                .MakeGenericMethod(messageType);
            var instantiatedEnvelope = instantiateMethod.Invoke(null, new object[] { envelope, decoder });

            try
            {
                var messageHandlerMethod = getHandlerMethod(handlerFrom);
                messageHandlerMethod.Invoke(handlerFrom, new object[] { instantiatedEnvelope });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("HandledBy, MessageType[{0}], ErrorMessage[{1}].", messageType.Name, ex);
                throw;
            }
        }

        public static void HandledBy<T>(this ActorMessageEnvelope envelope, object handlerFrom, Type messageType, IActorMessageDecoder decoder, T state) where T : class
        {
            HandledBy<T>(envelope, handlerFrom, messageType, decoder, state, (string s) => { return @"On" + s; });
        }

        public static void HandledBy<T>(this ActorMessageEnvelope envelope, object handlerFrom, Type messageType, IActorMessageDecoder decoder, T state, Func<string, string> getHandlerName) where T : class
        {
            HandledBy<T>(envelope, handlerFrom, messageType, decoder, state,
                (object o) =>
                {
                    return o.GetType().GetMethod(getHandlerName(envelope.MessageType), BindingFlags.NonPublic | BindingFlags.Instance);
                });
        }

        public static void HandledBy<T>(this ActorMessageEnvelope envelope, object handlerFrom, Type messageType, IActorMessageDecoder decoder, T state, Func<object, MethodInfo> getHandlerMethod) where T : class
        {
            var instantiateMethod = typeof(ActorMessageEnvelopeExtensions)
                .GetMethod("Instantiate", new Type[] { typeof(ActorMessageEnvelope), typeof(IActorMessageDecoder) })
                .MakeGenericMethod(messageType);
            var instantiatedEnvelope = instantiateMethod.Invoke(null, new object[] { envelope, decoder });

            try
            {
                var messageHandlerMethod = getHandlerMethod(handlerFrom);
                messageHandlerMethod.Invoke(handlerFrom, new object[] { state, instantiatedEnvelope });
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("HandledBy, MessageType[{0}], ErrorMessage[{1}].", messageType.Name, ex);
                throw;
            }
        }
    }
}
