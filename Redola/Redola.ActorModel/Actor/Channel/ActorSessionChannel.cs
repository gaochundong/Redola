using System;
using System.Threading;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorSessionChannel : IActorChannel, IDisposable
    {
        private ILog _log = Logger.Get<ActorSessionChannel>();
        private ActorIdentity _localActor = null;
        private ActorChannelConfiguration _channelConfiguration = null;
        private ActorTransportSession _innerSession = null;
        private ActorIdentity _remoteActor = null;

        private readonly SemaphoreSlim _keepAliveLocker = new SemaphoreSlim(1, 1);
        private KeepAliveTracker _keepAliveTracker;
        private Timer _keepAliveTimeoutTimer;
        private bool _disposed = false;

        public ActorSessionChannel(
            ActorIdentity localActor,
            ActorChannelConfiguration channelConfiguration,
            ActorTransportSession session)
        {
            _localActor = localActor;
            _channelConfiguration = channelConfiguration;
            _innerSession = session;
            this.SessionKey = _innerSession.SessionKey;

            _keepAliveTracker = KeepAliveTracker.Create(KeepAliveInterval, new TimerCallback((s) => OnKeepAlive()));
            _keepAliveTimeoutTimer = new Timer(new TimerCallback((s) => OnKeepAliveTimeout()), null, Timeout.Infinite, Timeout.Infinite);
        }

        public string SessionKey { get; private set; }

        public string Identifier
        {
            get { return string.Format("Session#{0}", this.SessionKey); }
        }

        public ActorIdentity LocalActor
        {
            get
            {
                return _localActor;
            }
        }

        public ActorIdentity RemoteActor
        {
            get
            {
                return _remoteActor;
            }
        }

        public bool Active
        {
            get
            {
                if (_innerSession == null)
                    return false;
                else
                    return _innerSession.Active && IsHandshaked;
            }
        }

        public bool IsHandshaked { get; private set; }

        public void OnDataReceived(object sender, byte[] data, int dataOffset, int dataLength)
        {
            if (!IsHandshaked)
            {
                Handshake(data, dataOffset, dataLength);
            }
            else
            {
                _keepAliveTracker.OnDataReceived();
                StopKeepAliveTimeoutTimer(); // intend to disable keep-alive timeout when receive anything

                ActorFrameHeader actorKeepAliveRequestFrameHeader = null;
                bool isHeaderDecoded = _channelConfiguration.FrameBuilder.TryDecodeFrameHeader(
                    data, dataOffset, dataLength,
                    out actorKeepAliveRequestFrameHeader);
                if (isHeaderDecoded && actorKeepAliveRequestFrameHeader.OpCode == OpCode.Ping)
                {
                    _log.DebugFormat("KeepAlive receive request from remote actor [{0}] to local actor [{1}].", _remoteActor, _localActor);

                    var actorKeepAliveResponse = new PongFrame();
                    var actorKeepAliveResponseBuffer = _channelConfiguration.FrameBuilder.EncodeFrame(actorKeepAliveResponse);

                    _log.DebugFormat("KeepAlive send response from local actor [{0}] to remote actor [{1}].", _localActor, _remoteActor);
                    _innerSession.Send(actorKeepAliveResponseBuffer);
                }
                else if (isHeaderDecoded && actorKeepAliveRequestFrameHeader.OpCode == OpCode.Pong)
                {
                    _log.DebugFormat("KeepAlive receive response from remote actor [{0}] to local actor [{1}].", _remoteActor, _localActor);
                    StopKeepAliveTimeoutTimer();
                }
                else
                {
                    if (SessionDataReceived != null)
                    {
                        SessionDataReceived(this, new ActorChannelSessionDataReceivedEventArgs(
                            this, _remoteActor, data, dataOffset, dataLength));
                    }
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
                    .DecodeFrameData<ActorIdentity>(payload, payloadOffset, payloadCount);

                _remoteActor = actorHandshakeRequestData;
            }

            if (_remoteActor == null)
            {
                _log.ErrorFormat("Handshake with remote [{0}] failed, invalid actor description.", this.SessionKey);
                Close();
            }
            else
            {
                var actorHandshakeResponseData = _channelConfiguration.FrameBuilder.ControlFrameDataEncoder.EncodeFrameData(_localActor);
                var actorHandshakeResponse = new WelcomeFrame(actorHandshakeResponseData);
                var actorHandshakeResponseBuffer = _channelConfiguration.FrameBuilder.EncodeFrame(actorHandshakeResponse);

                _innerSession.Send(actorHandshakeResponseBuffer);
                IsHandshaked = true;

                _log.DebugFormat("Handshake with remote [{0}] successfully, SessionKey[{1}].", _remoteActor, this.SessionKey);
                if (SessionHandshaked != null)
                {
                    SessionHandshaked(this, new ActorChannelSessionHandshakedEventArgs(this, _remoteActor));
                }

                _keepAliveTracker.StartTimer();
            }
        }

        protected event EventHandler<ActorChannelSessionHandshakedEventArgs> SessionHandshaked;
        protected event EventHandler<ActorChannelSessionDataReceivedEventArgs> SessionDataReceived;

        private void OnSessionHandshaked(object sender, ActorChannelSessionHandshakedEventArgs e)
        {
            if (ChannelConnected != null)
            {
                ChannelConnected(this, new ActorChannelConnectedEventArgs(this.Identifier, e.RemoteActor));
            }
        }

        private void OnSessionDataReceived(object sender, ActorChannelSessionDataReceivedEventArgs e)
        {
            if (ChannelDataReceived != null)
            {
                ChannelDataReceived(this, new ActorChannelDataReceivedEventArgs(
                    this.Identifier, e.RemoteActor, e.Data, e.DataOffset, e.DataLength));
            }
        }

        public event EventHandler<ActorChannelConnectedEventArgs> ChannelConnected;
        public event EventHandler<ActorChannelDisconnectedEventArgs> ChannelDisconnected;
        public event EventHandler<ActorChannelDataReceivedEventArgs> ChannelDataReceived;

        public void Open()
        {
            this.SessionHandshaked += OnSessionHandshaked;
            this.SessionDataReceived += OnSessionDataReceived;
        }

        public void Close()
        {
            try
            {
                if (_keepAliveTracker != null)
                {
                    _keepAliveTracker.StopTimer();
                }
                if (_keepAliveTimeoutTimer != null)
                {
                    _keepAliveTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }

                var copySession = _innerSession;
                _innerSession = null;
                if (copySession != null && copySession.Active)
                {
                    copySession.Close();
                }

                this.SessionHandshaked -= OnSessionHandshaked;
                this.SessionDataReceived -= OnSessionDataReceived;

                if (ChannelDisconnected != null)
                {
                    ChannelDisconnected(this, new ActorChannelDisconnectedEventArgs(this.Identifier, _remoteActor));
                }
            }
            finally
            {
                _remoteActor = null;
                IsHandshaked = false;
            }
        }

        #region Send

        public void Send(string identifier, byte[] data)
        {
            Send(identifier, data, 0, data.Length);
        }

        public void Send(string identifier, byte[] data, int offset, int count)
        {
            if (this.Identifier != identifier)
                throw new InvalidOperationException(
                    string.Format("Channel identifier is not matched, Identifier[{0}].", identifier));

            _innerSession.Send(data, offset, count);
            _keepAliveTracker.OnDataSent();
        }

        public void BeginSend(string identifier, byte[] data)
        {
            BeginSend(identifier, data, 0, data.Length);
        }

        public void BeginSend(string identifier, byte[] data, int offset, int count)
        {
            if (this.Identifier != identifier)
                throw new InvalidOperationException(
                    string.Format("Channel identifier is not matched, Identifier[{0}].", identifier));

            _innerSession.BeginSend(data, offset, count);
            _keepAliveTracker.OnDataSent();
        }

        public IAsyncResult BeginSend(string identifier, byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(identifier, data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(string identifier, byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            if (this.Identifier != identifier)
                throw new InvalidOperationException(
                    string.Format("Channel identifier is not matched, Identifier[{0}].", identifier));

            var ar = _innerSession.BeginSend(data, offset, count, callback, state);
            _keepAliveTracker.OnDataSent();

            return ar;
        }

        public void EndSend(string identifier, IAsyncResult asyncResult)
        {
            if (this.Identifier != identifier)
                throw new InvalidOperationException(
                    string.Format("Channel identifier is not matched, Identifier[{0}].", identifier));

            _innerSession.EndSend(asyncResult);
        }

        #endregion

        #region Keep Alive

        public TimeSpan KeepAliveInterval { get { return _channelConfiguration.KeepAliveInterval; } }

        public TimeSpan KeepAliveTimeout { get { return _channelConfiguration.KeepAliveTimeout; } }

        public bool KeepAliveEnabled { get { return _channelConfiguration.KeepAliveEnabled; } }

        private void StartKeepAliveTimeoutTimer()
        {
            _keepAliveTimeoutTimer.Change((int)KeepAliveTimeout.TotalMilliseconds, Timeout.Infinite);
        }

        private void StopKeepAliveTimeoutTimer()
        {
            _keepAliveTimeoutTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnKeepAliveTimeout()
        {
            _log.ErrorFormat("Keep-alive timer timeout [{0}].", KeepAliveTimeout);
            Close();
        }

        private void OnKeepAlive()
        {
            if (_keepAliveLocker.Wait(0))
            {
                try
                {
                    if (!Active)
                        return;

                    if (!KeepAliveEnabled)
                        return;

                    if (_localActor.Type == _remoteActor.Type
                        && _localActor.Name == _remoteActor.Name)
                        return;

                    if (_keepAliveTracker.ShouldSendKeepAlive())
                    {
                        var actorKeepAliveRequest = new PingFrame();
                        var actorKeepAliveRequestBuffer = _channelConfiguration.FrameBuilder.EncodeFrame(actorKeepAliveRequest);

                        _log.DebugFormat("KeepAlive send request from local actor [{0}] to remote actor [{1}].", _localActor, _remoteActor);

                        _innerSession.Send(actorKeepAliveRequestBuffer);
                        StartKeepAliveTimeoutTimer();

                        _keepAliveTracker.ResetTimer();
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(ex.Message, ex);
                    Close();
                }
                finally
                {
                    _keepAliveLocker.Release();
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (_keepAliveTracker != null)
                        {
                            _keepAliveTracker.Dispose();
                        }
                        if (_keepAliveTimeoutTimer != null)
                        {
                            _keepAliveTimeoutTimer.Dispose();
                        }
                    }
                    catch { }
                }

                _disposed = true;
            }
        }

        #endregion

        public override string ToString()
        {
            return this.Identifier;
        }
    }
}
