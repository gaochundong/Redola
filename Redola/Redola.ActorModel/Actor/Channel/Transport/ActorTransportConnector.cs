using System;
using System.Net;
using Cowboy.Sockets;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class ActorTransportConnector
    {
        private ILog _log = Logger.Get<ActorTransportConnector>();
        private TcpSocketClient _client;

        public ActorTransportConnector(IPEndPoint connectToEndPoint, ActorTransportConfiguration transportConfiguration)
        {
            if (connectToEndPoint == null)
                throw new ArgumentNullException("connectToEndPoint");
            if (transportConfiguration == null)
                throw new ArgumentNullException("transportConfiguration");

            this.ConnectToEndPoint = connectToEndPoint;
            this.TransportConfiguration = transportConfiguration;
        }

        public IPEndPoint ConnectToEndPoint { get; private set; }
        public ActorTransportConfiguration TransportConfiguration { get; private set; }
        public bool IsConnected { get { return _client == null ? false : _client.State == TcpSocketConnectionState.Connected; } }

        public void Connect()
        {
            if (IsConnected)
                return;
            if (this.ConnectToEndPoint.Address.Equals(IPAddress.None)
                || this.ConnectToEndPoint.Address.Equals(IPAddress.IPv6None))
                return;

            try
            {
                var configuration = new TcpSocketClientConfiguration()
                {
                    ConnectTimeout = this.TransportConfiguration.ConnectTimeout,
                    ReceiveBufferSize = this.TransportConfiguration.ReceiveBufferSize,
                    SendBufferSize = this.TransportConfiguration.SendBufferSize,
                    ReceiveTimeout = this.TransportConfiguration.ReceiveTimeout,
                    SendTimeout = this.TransportConfiguration.SendTimeout,
                    NoDelay = this.TransportConfiguration.NoDelay,
                    LingerState = this.TransportConfiguration.LingerState,
                    KeepAlive = this.TransportConfiguration.KeepAlive,
                    KeepAliveInterval = this.TransportConfiguration.KeepAliveInterval,
                    ReuseAddress = this.TransportConfiguration.ReuseAddress,
                };
                _client = new TcpSocketClient(this.ConnectToEndPoint, configuration);
                _client.ServerConnected += OnServerConnected;
                _client.ServerDisconnected += OnServerDisconnected;
                _client.ServerDataReceived += OnServerDataReceived;

                _log.DebugFormat("TCP client is connecting to [{0}].", this.ConnectToEndPoint);
                _client.Connect();

                OnConnect();
            }
            catch
            {
                _client.Close();
                _client.ServerConnected -= OnServerConnected;
                _client.ServerDisconnected -= OnServerDisconnected;
                _client.ServerDataReceived -= OnServerDataReceived;
                _client = null;

                throw;
            }
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;
            if (this.ConnectToEndPoint.Address.Equals(IPAddress.None)
                || this.ConnectToEndPoint.Address.Equals(IPAddress.IPv6None))
                return;

            try
            {
                _client.Close();
                _client.ServerConnected -= OnServerConnected;
                _client.ServerDisconnected -= OnServerDisconnected;
                _client.ServerDataReceived -= OnServerDataReceived;
                _client = null;
            }
            catch { }
            finally
            {
                OnDisconnect();
            }
        }

        protected virtual void OnConnect()
        {
        }

        protected virtual void OnDisconnect()
        {
        }

        protected virtual void OnServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            _log.DebugFormat("TCP server [{0}] has connected.", e);

            if (TransportConnected != null)
            {
                TransportConnected(this, new ActorTransportConnectedEventArgs(this.ConnectToEndPoint.ToString()));
            }
        }

        protected virtual void OnServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            _log.DebugFormat("TCP server [{0}] has disconnected.", e);

            if (TransportDisconnected != null)
            {
                TransportDisconnected(this, new ActorTransportDisconnectedEventArgs(this.ConnectToEndPoint.ToString()));
            }
        }

        protected virtual void OnServerDataReceived(object sender, TcpServerDataReceivedEventArgs e)
        {
            if (TransportDataReceived != null)
            {
                TransportDataReceived(this, new ActorTransportDataReceivedEventArgs(this.ConnectToEndPoint.ToString(), e.Data, e.DataOffset, e.DataLength));
            }
        }

        public void Send(byte[] data)
        {
            Send(data, 0, data.Length);
        }

        public void Send(byte[] data, int offset, int count)
        {
            if (!IsConnected)
                throw new InvalidOperationException("The client has not connected to server.");

            _client.Send(data, offset, count);
        }

        public void BeginSend(byte[] data)
        {
            BeginSend(data, 0, data.Length);
        }

        public void BeginSend(byte[] data, int offset, int count)
        {
            if (!IsConnected)
                throw new InvalidOperationException("The client has not connected to server.");

            _client.BeginSend(data, offset, count);
        }

        public IAsyncResult BeginSend(byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            if (!IsConnected)
                throw new InvalidOperationException("The client has not connected to server.");

            return _client.BeginSend(data, offset, count, callback, state);
        }

        public void EndSend(IAsyncResult asyncResult)
        {
            _client.EndSend(asyncResult);
        }

        public event EventHandler<ActorTransportConnectedEventArgs> TransportConnected;
        public event EventHandler<ActorTransportDisconnectedEventArgs> TransportDisconnected;
        public event EventHandler<ActorTransportDataReceivedEventArgs> TransportDataReceived;
    }
}
