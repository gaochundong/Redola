using System;
using System.Threading;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorChannelSession : IDisposable
    {
        private ILog _log = Logger.Get<ActorChannelSession>();
        private ActorIdentity _localActor = null;
        private ActorChannelConfiguration _channelConfiguration = null;
        private ActorTransportSession _innerSession = null;
        private ActorIdentity _remoteActor = null;

        private readonly SemaphoreSlim _keepAliveLocker = new SemaphoreSlim(1, 1);
        private KeepAliveTracker _keepAliveTracker;
        private Timer _keepAliveTimeoutTimer;
        private bool _disposed = false;

        public ActorChannelSession(
            ActorIdentity localActor,
            ActorChannelConfiguration channelConfiguration,
            ActorTransportSession session)
        {
            _localActor = localActor;
            _channelConfiguration = channelConfiguration;
            _innerSession = session;

            _keepAliveTracker = KeepAliveTracker.Create(KeepAliveInterval, new TimerCallback((s) => OnKeepAlive()));
            _keepAliveTimeoutTimer = new Timer(new TimerCallback((s) => OnKeepAliveTimeout()), null, Timeout.Infinite, Timeout.Infinite);
        }

        public string SessionKey { get { return _innerSession.SessionKey; } }

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

        public void OnDataReceived(byte[] data, int dataOffset, int dataLength)
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
                    if (DataReceived != null)
                    {
                        DataReceived(this, new ActorChannelSessionDataReceivedEventArgs(
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
                if (Handshaked != null)
                {
                    Handshaked(this, new ActorChannelSessionHandshakedEventArgs(this, _remoteActor));
                }

                _keepAliveTracker.StartTimer();
            }
        }

        public event EventHandler<ActorChannelSessionHandshakedEventArgs> Handshaked;
        public event EventHandler<ActorChannelSessionDataReceivedEventArgs> DataReceived;

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
                if (copySession != null)
                {
                    copySession.Close();
                }
            }
            finally
            {
                _remoteActor = null;
                IsHandshaked = false;
            }
        }

        public void Send(string actorType, string actorName, byte[] data)
        {
            Send(actorType, actorName, data, 0, data.Length);
        }

        public void Send(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);

            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}], Name[{1}].", actorType, actorName));
            if (_remoteActor.GetKey() != actorKey)
                throw new InvalidOperationException(
                    string.Format("Remote actor key not matched, [{0}]:[{1}].", _remoteActor.GetKey(), actorKey));

            _innerSession.Send(data, offset, count);
            _keepAliveTracker.OnDataSent();
        }

        public void BeginSend(string actorType, string actorName, byte[] data)
        {
            BeginSend(actorType, actorName, data, 0, data.Length);
        }

        public void BeginSend(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);

            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}], Name[{1}].", actorType, actorName));
            if (_remoteActor.GetKey() != actorKey)
                throw new InvalidOperationException(
                    string.Format("Remote actor key not matched, [{0}]:[{1}].", _remoteActor.GetKey(), actorKey));

            _innerSession.BeginSend(data, offset, count);
            _keepAliveTracker.OnDataSent();
        }

        public void Send(string actorType, byte[] data)
        {
            Send(actorType, data, 0, data.Length);
        }

        public void Send(string actorType, byte[] data, int offset, int count)
        {
            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}].", actorType));
            if (_remoteActor.Type != actorType)
                throw new InvalidOperationException(
                    string.Format("Remote actor type not matched, [{0}]:[{1}].", _remoteActor.Type, actorType));

            _innerSession.Send(data, offset, count);
            _keepAliveTracker.OnDataSent();
        }

        public void BeginSend(string actorType, byte[] data)
        {
            BeginSend(actorType, data, 0, data.Length);
        }

        public void BeginSend(string actorType, byte[] data, int offset, int count)
        {
            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}].", actorType));
            if (_remoteActor.Type != actorType)
                throw new InvalidOperationException(
                    string.Format("Remote actor type not matched, [{0}]:[{1}].", _remoteActor.Type, actorType));

            _innerSession.BeginSend(data, offset, count);
            _keepAliveTracker.OnDataSent();
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(actorType, actorName, data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);

            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}], Name[{1}].", actorType, actorName));
            if (_remoteActor.GetKey() != actorKey)
                throw new InvalidOperationException(
                    string.Format("Remote actor key not matched, [{0}]:[{1}].", _remoteActor.GetKey(), actorKey));

            var ar = _innerSession.BeginSend(data, offset, count, callback, state);
            _keepAliveTracker.OnDataSent();

            return ar;
        }

        public void EndSend(string actorType, string actorName, IAsyncResult asyncResult)
        {
            _innerSession.EndSend(asyncResult);
        }

        #region Keep Alive

        public TimeSpan KeepAliveInterval { get { return _channelConfiguration.KeepAliveInterval; } }

        public TimeSpan KeepAliveTimeout { get { return _channelConfiguration.KeepAliveTimeout; } }

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
    }
}
