using System;
using System.Collections.Concurrent;
using System.Net;
using Cowboy.Sockets;
using Logrila.Logging;
using Redola.ActorModel.Extensions;

namespace Redola.ActorModel
{
    public class ActorTransportListener
    {
        private ILog _log = Logger.Get<ActorTransportListener>();
        private TcpSocketServer _server;
        private ConcurrentDictionary<string, TcpSocketSession> _sessions = new ConcurrentDictionary<string, TcpSocketSession>(); // sessionKey -> session

        public ActorTransportListener(IPEndPoint listenedEndPoint)
        {
            if (listenedEndPoint == null)
                throw new ArgumentNullException("listenedEndPoint");

            this.ListenedEndPoint = listenedEndPoint;
        }

        public IPEndPoint ListenedEndPoint { get; private set; }
        public bool IsListening { get { return _server == null ? false : _server.IsListening; } }

        public void Start()
        {
            if (IsListening)
                return;
            if (this.ListenedEndPoint.Address.Equals(IPAddress.None)
                || this.ListenedEndPoint.Address.Equals(IPAddress.IPv6None))
                return;

            try
            {
                var configuration = new TcpSocketServerConfiguration()
                {
                    SendTimeout = TimeSpan.FromMinutes(1),
                    ReceiveTimeout = TimeSpan.Zero,
                };
                _server = new TcpSocketServer(this.ListenedEndPoint, configuration);
                _server.ClientConnected += OnClientConnected;
                _server.ClientDisconnected += OnClientDisconnected;
                _server.ClientDataReceived += OnClientDataReceived;

                _log.InfoFormat("TCP server is listening to [{0}].", this.ListenedEndPoint);
                _server.Listen();
            }
            catch
            {
                _server.ClientConnected -= OnClientConnected;
                _server.ClientDisconnected -= OnClientDisconnected;
                _server.ClientDataReceived -= OnClientDataReceived;
                _server.Shutdown();
                _server = null;

                throw;
            }
        }

        public void Stop()
        {
            if (!IsListening)
                return;
            if (this.ListenedEndPoint.Address.Equals(IPAddress.None)
                || this.ListenedEndPoint.Address.Equals(IPAddress.IPv6None))
                return;

            try
            {
                _server.ClientConnected -= OnClientConnected;
                _server.ClientDisconnected -= OnClientDisconnected;
                _server.ClientDataReceived -= OnClientDataReceived;
                _server.Shutdown();
                _server = null;
            }
            catch { }
        }

        public void CloseSession(string sessionKey)
        {
            _server.CloseSession(sessionKey);
        }

        private void OnClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            _log.InfoFormat("TCP client [{0}] has connected.", e.Session.RemoteEndPoint);
            _sessions.Add(e.Session.SessionKey, e.Session);

            if (Connected != null)
            {
                Connected(this, new ActorTransportConnectedEventArgs(e.Session.SessionKey));
            }
        }

        private void OnClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            _log.InfoFormat("TCP client [{0}] has disconnected.", e.Session.RemoteEndPoint);
            _sessions.Remove(e.Session.SessionKey);

            if (Disconnected != null)
            {
                Disconnected(this, new ActorTransportDisconnectedEventArgs(e.Session.SessionKey));
            }
        }

        private void OnClientDataReceived(object sender, TcpClientDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new ActorTransportDataReceivedEventArgs(e.Session.SessionKey, e.Data, e.DataOffset, e.DataLength));
            }
        }

        public void SendTo(string sessionKey, byte[] data)
        {
            if (!IsListening)
                throw new InvalidOperationException("The server has stopped to listen.");

            TcpSocketSession session = null;
            if (_sessions.TryGetValue(sessionKey, out session))
            {
                _server.SendTo(session, data);
            }
            else
            {
                _log.WarnFormat("SendTo, cannot find target client [{0}].", sessionKey);
            }
        }

        public void SendTo(string sessionKey, byte[] data, int offset, int count)
        {
            if (!IsListening)
                throw new InvalidOperationException("The server has stopped to listen.");

            TcpSocketSession session = null;
            if (_sessions.TryGetValue(sessionKey, out session))
            {
                _server.SendTo(session, data, offset, count);
            }
            else
            {
                _log.WarnFormat("SendTo, cannot find target client [{0}].", sessionKey);
            }
        }

        public void SendToAsync(string sessionKey, byte[] data)
        {
            if (!IsListening)
                throw new InvalidOperationException("The server has stopped to listen.");

            TcpSocketSession session = null;
            if (_sessions.TryGetValue(sessionKey, out session))
            {
                _server.SendToAsync(session, data);
            }
            else
            {
                _log.WarnFormat("SendToAsync, cannot find target client [{0}].", sessionKey);
            }
        }

        public void SendToAsync(string sessionKey, byte[] data, int offset, int count)
        {
            if (!IsListening)
                throw new InvalidOperationException("The server has stopped to listen.");

            TcpSocketSession session = null;
            if (_sessions.TryGetValue(sessionKey, out session))
            {
                _server.SendToAsync(session, data, offset, count);
            }
            else
            {
                _log.WarnFormat("SendToAsync, cannot find target client [{0}].", sessionKey);
            }
        }

        public void Broadcast(byte[] data)
        {
            if (!IsListening)
                throw new InvalidOperationException("The server has stopped to listen.");

            _server.Broadcast(data);
        }

        public void BroadcastAsync(byte[] data)
        {
            if (!IsListening)
                throw new InvalidOperationException("The server has stopped to listen.");

            _server.BroadcastAsync(data);
        }

        public event EventHandler<ActorTransportConnectedEventArgs> Connected;
        public event EventHandler<ActorTransportDisconnectedEventArgs> Disconnected;
        public event EventHandler<ActorTransportDataReceivedEventArgs> DataReceived;
    }
}
