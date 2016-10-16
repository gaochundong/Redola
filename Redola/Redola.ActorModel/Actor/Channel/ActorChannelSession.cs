using System;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorChannelSession
    {
        private ILog _log = Logger.Get<ActorChannelSession>();
        private ActorDescription _localActor = null;
        private ActorChannelConfiguration _channelConfiguration = null;
        private ActorTransportSession _innerSession = null;
        private ActorDescription _remoteActor = null;

        public ActorChannelSession(
            ActorDescription localActor,
            ActorChannelConfiguration channelConfiguration,
            ActorTransportSession session)
        {
            _localActor = localActor;
            _channelConfiguration = channelConfiguration;
            _innerSession = session;
        }

        public string SessionKey { get { return _innerSession.SessionKey; } }
        public bool IsHandshaked { get; private set; }

        internal void OnDataReceived(byte[] data, int dataOffset, int dataLength)
        {
            if (!IsHandshaked)
            {
                Handshake(data, dataOffset, dataLength);
            }
            else
            {
                if (DataReceived != null)
                {
                    DataReceived(this, new ActorChannelSessionDataReceivedEventArgs(
                        this, _remoteActor, data, dataOffset, dataLength));
                }
            }
        }

        private void Handshake(byte[] data, int dataOffset, int dataLength)
        {
            ActorFrameHeader actorHandshakeRequestFrameHeader = null;
            bool isHeaderDecoded = _channelConfiguration.FrameBuilder.TryDecodeFrameHeader(
                data, dataOffset, dataLength,
                out actorHandshakeRequestFrameHeader);
            if (isHeaderDecoded && actorHandshakeRequestFrameHeader.OpCode == OpCode.Hello)
            {
                byte[] payload;
                int payloadOffset;
                int payloadCount;
                _channelConfiguration.FrameBuilder.DecodePayload(
                    data, dataOffset, actorHandshakeRequestFrameHeader,
                    out payload, out payloadOffset, out payloadCount);
                var actorHandshakeRequestData =
                    _channelConfiguration
                    .FrameBuilder
                    .ControlFrameDataDecoder
                    .DecodeFrameData<ActorDescription>(payload, payloadOffset, payloadCount);

                _remoteActor = actorHandshakeRequestData;
            }

            if (_remoteActor == null)
            {
                _log.ErrorFormat("Handshake with remote [{0}] failed, invalid actor description.", this.SessionKey);
                _innerSession.Close();
            }
            else
            {
                var actorHandshakeResponseData = _channelConfiguration.FrameBuilder.ControlFrameDataEncoder.EncodeFrameData(_localActor);
                var actorHandshakeResponse = new WelcomeFrame(actorHandshakeResponseData);
                var actorHandshakeResponseBuffer = _channelConfiguration.FrameBuilder.EncodeFrame(actorHandshakeResponse);

                _innerSession.BeginSend(actorHandshakeResponseBuffer);

                _log.InfoFormat("Handshake with remote [{0}] successfully, SessionKey[{1}].", _remoteActor, this.SessionKey);
                if (Handshaked != null)
                {
                    Handshaked(this, new ActorChannelSessionHandshakedEventArgs(this, _remoteActor));
                }
            }
        }

        public event EventHandler<ActorChannelSessionHandshakedEventArgs> Handshaked;
        public event EventHandler<ActorChannelSessionDataReceivedEventArgs> DataReceived;
    }
}
