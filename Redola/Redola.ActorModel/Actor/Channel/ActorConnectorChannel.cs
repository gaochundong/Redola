using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorConnectorChannel : IActorChannel
    {
        private ILog _log = Logger.Get<ActorConnectorChannel>();
        private ActorDescription _localActor;
        private ActorDescription _remoteActor;
        private ActorTransportConnector _connector;
        private IActorFrameBuilder _frameBuilder;
        private bool _isHandshaked = false;

        public ActorConnectorChannel(
            ActorDescription localActor,
            ActorTransportConnector remoteConnector,
            IActorFrameBuilder frameBuilder)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            if (remoteConnector == null)
                throw new ArgumentNullException("remoteConnector");
            if (frameBuilder == null)
                throw new ArgumentNullException("frameBuilder");

            _localActor = localActor;
            _connector = remoteConnector;
            _frameBuilder = frameBuilder;
        }

        public bool Active
        {
            get
            {
                if (_connector == null)
                    return false;
                else
                    return _connector.IsConnected && _isHandshaked;
            }
        }

        public IPEndPoint ConnectToEndPoint
        {
            get
            {
                return _connector.ConnectToEndPoint;
            }
        }

        public void Open()
        {
            Open(TimeSpan.FromSeconds(5));
        }

        public void Open(TimeSpan timeout)
        {
            try
            {
                if (_connector.IsConnected)
                    return;

                _connector.Connected += OnConnected;
                _connector.Disconnected += OnDisconnected;

                _connector.Connect(timeout);

                OnOpen();
            }
            catch (TimeoutException)
            {
                _log.ErrorFormat("Connect remote [{0}] timeout with [{1}].", this.ConnectToEndPoint, timeout);
                Close();
            }
        }

        public void Close()
        {
            try
            {
                _connector.Connected -= OnConnected;
                _connector.Disconnected -= OnDisconnected;
                _connector.DataReceived -= OnDataReceived;

                if (_connector.IsConnected)
                {
                    _connector.Disconnect();
                }

                if (Disconnected != null)
                {
                    Disconnected(this, new ActorDisconnectedEventArgs(this.ConnectToEndPoint.ToString(), _remoteActor));
                }

                _log.InfoFormat("Disconnected with remote [{0}], SessionKey[{1}].", _remoteActor, this.ConnectToEndPoint);
            }
            finally
            {
                _remoteActor = null;
                _isHandshaked = false;
                OnClose();
            }
        }

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }

        private void Handshake()
        {
            Handshake(TimeSpan.FromSeconds(5));
        }

        private void Handshake(TimeSpan timeout)
        {
            var actorHandshakeRequestData = _encoder.EncodeMessage(_localActor);
            var actorHandshakeRequest = new HelloFrame(actorHandshakeRequestData);
            var actorHandshakeRequestBuffer = _frameBuilder.EncodeFrame(actorHandshakeRequest);

            ManualResetEventSlim waitingHandshaked = new ManualResetEventSlim(false);
            ActorTransportDataReceivedEventArgs handshakeResponseEvent = null;
            EventHandler<ActorTransportDataReceivedEventArgs> onHandshaked =
                (s, e) =>
                {
                    handshakeResponseEvent = e;
                    waitingHandshaked.Set();
                };

            _connector.DataReceived += onHandshaked;
            _log.InfoFormat("Handshake request from local actor [{0}].", _localActor);
            _connector.BeginSend(actorHandshakeRequestBuffer);

            bool handshaked = waitingHandshaked.Wait(timeout);
            _connector.DataReceived -= onHandshaked;
            waitingHandshaked.Dispose();

            if (handshaked && handshakeResponseEvent != null)
            {
                ActorFrameHeader actorHandshakeResponseFrameHeader = null;
                bool isHeaderDecoded = _frameBuilder.TryDecodeFrameHeader(
                    handshakeResponseEvent.Data, handshakeResponseEvent.DataOffset, handshakeResponseEvent.DataLength,
                    out actorHandshakeResponseFrameHeader);
                if (isHeaderDecoded && actorHandshakeResponseFrameHeader.OpCode == OpCode.Welcome)
                {
                    byte[] payload;
                    int payloadOffset;
                    int payloadCount;
                    _frameBuilder.DecodePayload(
                        handshakeResponseEvent.Data, handshakeResponseEvent.DataOffset, actorHandshakeResponseFrameHeader,
                        out payload, out payloadOffset, out payloadCount);
                    var actorHandshakeResponseData = _decoder.DecodeMessage<ActorDescription>(
                        payload, payloadOffset, payloadCount);

                    _remoteActor = actorHandshakeResponseData;
                    _log.InfoFormat("Handshake response from remote actor [{0}].", _remoteActor);
                }

                if (_remoteActor == null)
                {
                    _log.ErrorFormat("Handshake with remote [{0}] failed, invalid actor description.", this.ConnectToEndPoint);
                    Close();
                }
                else
                {
                    _log.InfoFormat("Handshake with remote [{0}] successfully, RemoteActor[{1}].", this.ConnectToEndPoint, _remoteActor);

                    _isHandshaked = true;
                    if (Connected != null)
                    {
                        Connected(this, new ActorConnectedEventArgs(this.ConnectToEndPoint.ToString(), _remoteActor));
                    }

                    _connector.DataReceived += OnDataReceived;
                }
            }
            else
            {
                _log.ErrorFormat("Handshake with remote [{0}] timeout [{1}].", this.ConnectToEndPoint, timeout);
                Close();
            }
        }

        protected virtual void OnConnected(object sender, ActorTransportConnectedEventArgs e)
        {
            Task.Run(() => { Handshake(); });
        }

        protected virtual void OnDisconnected(object sender, ActorTransportDisconnectedEventArgs e)
        {
            Close();
        }

        protected virtual void OnDataReceived(object sender, ActorTransportDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new ActorDataReceivedEventArgs(this.ConnectToEndPoint.ToString(), _remoteActor, e.Data, e.DataOffset, e.DataLength));
            }
        }

        public event EventHandler<ActorConnectedEventArgs> Connected;
        public event EventHandler<ActorDisconnectedEventArgs> Disconnected;
        public event EventHandler<ActorDataReceivedEventArgs> DataReceived;

        public void Send(string actorType, string actorName, byte[] data)
        {
            Send(actorType, actorName, data, 0, data.Length);
        }

        public void Send(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorDescription.GetKey(actorType, actorName);

            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}], Name[{1}].", actorType, actorName));
            if (_remoteActor.GetKey() != actorKey)
                throw new InvalidOperationException(
                    string.Format("Remote actor key not matched, [{0}]:[{1}].", _remoteActor.GetKey(), actorKey));

            _connector.Send(data, offset, count);
        }

        public void BeginSend(string actorType, string actorName, byte[] data)
        {
            BeginSend(actorType, actorName, data, 0, data.Length);
        }

        public void BeginSend(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorDescription.GetKey(actorType, actorName);

            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}], Name[{1}].", actorType, actorName));
            if (_remoteActor.GetKey() != actorKey)
                throw new InvalidOperationException(
                    string.Format("Remote actor key not matched, [{0}]:[{1}].", _remoteActor.GetKey(), actorKey));

            _connector.BeginSend(data, offset, count);
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

            _connector.Send(data, offset, count);
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

            _connector.BeginSend(data, offset, count);
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(actorType, actorName, data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            var actorKey = ActorDescription.GetKey(actorType, actorName);

            if (_remoteActor == null)
                throw new InvalidOperationException(
                    string.Format("The remote actor has not been connected, Type[{0}], Name[{1}].", actorType, actorName));
            if (_remoteActor.GetKey() != actorKey)
                throw new InvalidOperationException(
                    string.Format("Remote actor key not matched, [{0}]:[{1}].", _remoteActor.GetKey(), actorKey));

            return _connector.BeginSend(data, offset, count, callback, state);
        }

        public void EndSend(string actorType, string actorName, IAsyncResult asyncResult)
        {
            _connector.EndSend(asyncResult);
        }
    }
}
