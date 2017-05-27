using System;
using System.Collections.Concurrent;
using System.Linq;
using Logrila.Logging;
using Redola.ActorModel.Extensions;

namespace Redola.ActorModel
{
    public class ActorListenerChannel : IActorChannel
    {
        private ILog _log = Logger.Get<ActorListenerChannel>();
        private ActorIdentity _localActor = null;
        private ActorTransportListener _listener = null;
        private ActorChannelConfiguration _channelConfiguration = null;

        private class SessionItem
        {
            public SessionItem() { }
            public SessionItem(string sessionKey, ActorChannelSession session)
            {
                this.SessionKey = sessionKey;
                this.Session = session;
            }

            public string SessionKey { get; set; }
            public ActorChannelSession Session { get; set; }

            public string RemoteActorKey { get; set; }
            public ActorIdentity RemoteActor { get; set; }

            public override string ToString()
            {
                return string.Format("{0}#{1}", SessionKey, RemoteActor);
            }
        }
        private ConcurrentDictionary<string, SessionItem> _sessions
            = new ConcurrentDictionary<string, SessionItem>(); // SessionKey -> SessionItem

        public ActorListenerChannel(
            ActorIdentity localActor,
            ActorTransportListener localListener,
            ActorChannelConfiguration channelConfiguration)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            if (localListener == null)
                throw new ArgumentNullException("localListener");
            if (channelConfiguration == null)
                throw new ArgumentNullException("channelConfiguration");

            _localActor = localActor;
            _listener = localListener;
            _channelConfiguration = channelConfiguration;
        }

        public string Identifier
        {
            get
            {
                return _listener.ListenedEndPoint.ToString();
            }
        }

        public bool Active
        {
            get
            {
                return _listener.IsListening;
            }
        }

        public void Open()
        {
            if (_listener.IsListening)
                return;

            _listener.TransportConnected += OnTransportConnected;
            _listener.TransportDisconnected += OnTransportDisconnected;
            _listener.TransportDataReceived += OnTransportDataReceived;

            _listener.Start();
        }

        public void Close()
        {
            _listener.TransportConnected -= OnTransportConnected;
            _listener.TransportDisconnected -= OnTransportDisconnected;
            _listener.TransportDataReceived -= OnTransportDataReceived;

            _listener.Stop();

            foreach (var item in _sessions.Values)
            {
                item.Session.Handshaked -= OnChannelSessionHandshaked;
                item.Session.DataReceived -= OnChannelSessionDataReceived;
                item.Session.Close();
            }
            _sessions.Clear();
        }

        private void OnTransportConnected(object sender, ActorTransportSessionConnectedEventArgs e)
        {
            var session = new ActorChannelSession(_localActor, _channelConfiguration, e.Session);
            session.Handshaked += OnChannelSessionHandshaked;
            session.DataReceived += OnChannelSessionDataReceived;
            _sessions.Add(session.SessionKey, new SessionItem(session.SessionKey, session));
        }

        private void OnTransportDisconnected(object sender, ActorTransportSessionDisconnectedEventArgs e)
        {
            SessionItem item = null;
            if (_sessions.TryRemove(e.SessionKey, out item))
            {
                item.Session.Handshaked -= OnChannelSessionHandshaked;
                item.Session.DataReceived -= OnChannelSessionDataReceived;
                item.Session.Close();

                _log.DebugFormat("Disconnected with remote [{0}], SessionKey[{1}].", item.RemoteActor, e.SessionKey);

                if (item.RemoteActor != null)
                {
                    if (ChannelDisconnected != null)
                    {
                        ChannelDisconnected(this, new ActorChannelDisconnectedEventArgs(e.SessionKey, item.RemoteActor));
                    }
                }
            }
        }

        private void OnTransportDataReceived(object sender, ActorTransportSessionDataReceivedEventArgs e)
        {
            SessionItem item = null;
            if (_sessions.TryGetValue(e.SessionKey, out item))
            {
                item.Session.OnDataReceived(e.Data, e.DataOffset, e.DataLength);
            }
        }

        private void OnChannelSessionHandshaked(object sender, ActorChannelSessionHandshakedEventArgs e)
        {
            SessionItem item = null;
            if (_sessions.TryGetValue(e.SessionKey, out item))
            {
                item.RemoteActorKey = e.RemoteActor.GetKey();
                item.RemoteActor = e.RemoteActor;

                if (ChannelConnected != null)
                {
                    ChannelConnected(this, new ActorChannelConnectedEventArgs(e.SessionKey, e.RemoteActor));
                }
            }
        }

        private void OnChannelSessionDataReceived(object sender, ActorChannelSessionDataReceivedEventArgs e)
        {
            if (ChannelDataReceived != null)
            {
                ChannelDataReceived(this, new ActorChannelDataReceivedEventArgs(
                    e.SessionKey, e.RemoteActor, e.Data, e.DataOffset, e.DataLength));
            }
        }

        public event EventHandler<ActorChannelConnectedEventArgs> ChannelConnected;
        public event EventHandler<ActorChannelDisconnectedEventArgs> ChannelDisconnected;
        public event EventHandler<ActorChannelDataReceivedEventArgs> ChannelDataReceived;

        public void Send(string actorType, string actorName, byte[] data)
        {
            Send(actorType, actorName, data, 0, data.Length);
        }

        public void Send(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);
            var item = _sessions.Values.FirstOrDefault(s => s.RemoteActorKey == actorKey);
            if (item != null)
            {
                item.Session.Send(actorType, actorName, data, offset, count);
            }
        }

        public void BeginSend(string actorType, string actorName, byte[] data)
        {
            BeginSend(actorType, actorName, data, 0, data.Length);
        }

        public void BeginSend(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);
            var item = _sessions.Values.FirstOrDefault(s => s.RemoteActorKey == actorKey);
            if (item != null)
            {
                item.Session.BeginSend(actorType, actorName, data, offset, count);
            }
        }

        public void Send(string actorType, byte[] data)
        {
            Send(actorType, data, 0, data.Length);
        }

        public void Send(string actorType, byte[] data, int offset, int count)
        {
            var item = _sessions.Values.Where(a => a.RemoteActor.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
            if (item != null)
            {
                item.Session.Send(actorType, data, offset, count);
            }
        }

        public void BeginSend(string actorType, byte[] data)
        {
            BeginSend(actorType, data, 0, data.Length);
        }

        public void BeginSend(string actorType, byte[] data, int offset, int count)
        {
            var item = _sessions.Values.Where(a => a.RemoteActor.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
            if (item != null)
            {
                item.Session.BeginSend(actorType, data, offset, count);
            }
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(actorType, actorName, data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);
            var item = _sessions.Values.FirstOrDefault(s => s.RemoteActorKey == actorKey);
            if (item != null)
            {
                return item.Session.BeginSend(actorType, actorName, data, offset, count, callback, state);
            }

            return null;
        }

        public void EndSend(string actorType, string actorName, IAsyncResult asyncResult)
        {
            var actorKey = ActorIdentity.GetKey(actorType, actorName);
            var item = _sessions.Values.FirstOrDefault(s => s.RemoteActorKey == actorKey);
            if (item != null)
            {
                _listener.EndSendTo(item.SessionKey, asyncResult);
            }
        }
    }
}
