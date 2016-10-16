using System;
using Redola.ActorModel.Serialization;

namespace Redola.ActorModel.Framing
{
    internal class XmlActorControlFrameDataDecoder : IActorControlFrameDataDecoder
    {
        private IMessageDecoder _decoder;

        public XmlActorControlFrameDataDecoder(IMessageDecoder decoder)
        {
            _decoder = decoder;
        }

        public T DecodeFrameData<T>(byte[] data, int offset, int count)
        {
            return _decoder.DecodeMessage<T>(data, offset, count);
        }
    }
}
