using System;
using System.Net;
using System.Threading;
using Cowboy.Sockets;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class ActorTransportReconnectableConnector : ActorTransportConnector
    {
        private ILog _log = Logger.Get<ActorTransportReconnectableConnector>();
        private System.Threading.Timer _retryTimer = null;
        private readonly object _retryLock = new object();

        public ActorTransportReconnectableConnector(IPEndPoint connectToEndPoint)
            : base(connectToEndPoint)
        {
            this.RetryPeriod = TimeSpan.FromSeconds(15);
        }

        public TimeSpan RetryPeriod { get; private set; }

        protected override void OnDisconnect()
        {
            base.OnDisconnect();
            CloseRetryTimer();
        }

        protected override void OnServerConnected(object sender, TcpServerConnectedEventArgs e)
        {
            base.OnServerConnected(sender, e);
            CloseRetryTimer();
        }

        protected override void OnServerDisconnected(object sender, TcpServerDisconnectedEventArgs e)
        {
            base.OnServerDisconnected(sender, e);
            SetupRetryTimer();
        }

        private void SetupRetryTimer()
        {
            lock (_retryLock)
            {
                _log.InfoFormat(string.Format("SetupRetryTimer, setup retry timer [{0}] when connect to [{1}].",
                    this.RetryPeriod, this.ConnectToEndPoint));

                if (_retryTimer == null)
                {
                    _retryTimer = new System.Threading.Timer(
                      (s) =>
                      {
                          try
                          {
                              Connect();
                          }
                          catch (Exception ex)
                          {
                              _log.Error(ex.Message, ex);
                          }
                      },
                      null, this.RetryPeriod, this.RetryPeriod);
                }
            }
        }

        private void CloseRetryTimer()
        {
            lock (_retryLock)
            {
                _log.InfoFormat(string.Format("CloseRetryTimer, close retry timer [{0}] to connect [{1}].",
                    this.RetryPeriod, this.ConnectToEndPoint));

                if (_retryTimer != null)
                {
                    _retryTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    _retryTimer.Dispose();
                    _retryTimer = null;
                }
            }
        }
    }
}
