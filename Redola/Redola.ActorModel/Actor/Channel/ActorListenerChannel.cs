using System;
using System.Collections.Concurrent;
using System.Linq;
using Logrila.Logging;
using Redola.ActorModel.Extensions;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorListenerChannel : IActorChannel
    {
        private ILog _log = Logger.Get<ActorListenerChannel>();
        private ActorDescription _localActor = null;
        private ActorTransportListener _listener = null;
        private ActorChannelConfiguration _channelConfiguration = null;
        private ConcurrentDictionary<string, ActorChannelSession> _sessions = new ConcurrentDictionary<string, ActorChannelSession>(); // SessionKey -> Session
        private ConcurrentDictionary<string, ActorDescription> _remoteActors = new ConcurrentDictionary<string, ActorDescription>(); // SessionKey -> Actor
        private ConcurrentDictionary<string, string> _actorKeys = new ConcurrentDictionary<string, string>(); // ActorKey -> SessionKey

        public ActorListenerChannel(
            ActorDescription localActor,
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

        public bool Active
        {
            get
            {
                if (_listener == null)
                    return false;
                else
                    return _listener.IsListening;
            }
        }

        public void Open()
        {
            if (_listener.IsListening)
                return;

            _listener.Connected += OnConnected;
            _listener.Disconnected += OnDisconnected;
            _listener.DataReceived += OnDataReceived;

            _listener.Start();
        }

        public void Close()
        {
            _listener.Connected -= OnConnected;
            _listener.Disconnected -= OnDisconnected;
            _listener.DataReceived -= OnDataReceived;

            _listener.Stop();

            _remoteActors.Clear();
            _actorKeys.Clear();
        }
        
        private void OnConnected(object sender, ActorTransportSessionConnectedEventArgs e)
        {
            var session = new ActorChannelSession(_localActor, _channelConfiguration, e.Session);
            session.Handshaked += OnSessionHandshaked;
            session.DataReceived += OnSessionDataReceived;
            _sessions.Add(session.SessionKey, session);
        }

        private void OnDisconnected(object sender, ActorTransportSessionDisconnectedEventArgs e)
        {
            ActorChannelSession session = null;
            if (_sessions.TryRemove(e.SessionKey, out session))
            {
                session.Handshaked -= OnSessionHandshaked;
                session.DataReceived -= OnSessionDataReceived;
            }

            ActorDescription remoteActor = null;
            if (_remoteActors.TryRemove(e.SessionKey, out remoteActor))
            {
                _actorKeys.Remove(remoteActor.GetKey());                
                _log.InfoFormat("Disconnected with remote [{0}], SessionKey[{1}].", remoteActor, e.SessionKey);

                if (Disconnected != null)
                {
                    Disconnected(this, new ActorDisconnectedEventArgs(e.SessionKey, remoteActor));
                }
            }
        }

        private void OnDataReceived(object sender, ActorTransportSessionDataReceivedEventArgs e)
        {
            ActorChannelSession session = null;
            if(_sessions.TryGetValue(e.SessionKey, out session))
            {
                session.OnDataReceived(e.Data, e.DataOffset, e.DataLength);
            }
        }

        private void OnSessionHandshaked(object sender, ActorChannelSessionHandshakedEventArgs e)
        {
            _remoteActors.Add(e.SessionKey, e.RemoteActor);
            _actorKeys.Add(e.RemoteActor.GetKey(), e.SessionKey);

            if (Connected != null)
            {
                Connected(this, new ActorConnectedEventArgs(e.SessionKey, e.RemoteActor));
            }
        }

        private void OnSessionDataReceived(object sender, ActorChannelSessionDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new ActorDataReceivedEventArgs(
                    e.SessionKey, e.RemoteActor, e.Data, e.DataOffset, e.DataLength));
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
            var sessionKey = _actorKeys.Get(actorKey);
            if (!string.IsNullOrEmpty(sessionKey))
            {
                _listener.SendTo(sessionKey, data, offset, count);
            }
        }

        public void BeginSend(string actorType, string actorName, byte[] data)
        {
            BeginSend(actorType, actorName, data, 0, data.Length);
        }

        public void BeginSend(string actorType, string actorName, byte[] data, int offset, int count)
        {
            var actorKey = ActorDescription.GetKey(actorType, actorName);
            var sessionKey = _actorKeys.Get(actorKey);
            if (!string.IsNullOrEmpty(sessionKey))
            {
                _listener.BeginSendTo(sessionKey, data, offset, count);
            }
        }

        public void Send(string actorType, byte[] data)
        {
            Send(actorType, data, 0, data.Length);
        }

        public void Send(string actorType, byte[] data, int offset, int count)
        {
            var actor = _remoteActors.Values.Where(a => a.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
            if (actor != null)
            {
                var sessionKey = _actorKeys.Get(actor.GetKey());
                _listener.SendTo(sessionKey, data, offset, count);
            }
        }

        public void BeginSend(string actorType, byte[] data)
        {
            BeginSend(actorType, data, 0, data.Length);
        }

        public void BeginSend(string actorType, byte[] data, int offset, int count)
        {
            var actor = _remoteActors.Values.Where(a => a.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
            if (actor != null)
            {
                var sessionKey = _actorKeys.Get(actor.GetKey());
                _listener.BeginSendTo(sessionKey, data, offset, count);
            }
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, AsyncCallback callback, object state)
        {
            return BeginSend(actorType, actorName, data, 0, data.Length, callback, state);
        }

        public IAsyncResult BeginSend(string actorType, string actorName, byte[] data, int offset, int count, AsyncCallback callback, object state)
        {
            var actorKey = ActorDescription.GetKey(actorType, actorName);
            var sessionKey = _actorKeys.Get(actorKey);
            if (!string.IsNullOrEmpty(sessionKey))
            {
                return _listener.BeginSendTo(sessionKey, data, offset, count, callback, state);
            }

            return null;
        }

        public void EndSend(string actorType, string actorName, IAsyncResult asyncResult)
        {
            var actorKey = ActorDescription.GetKey(actorType, actorName);
            var sessionKey = _actorKeys.Get(actorKey);
            if (!string.IsNullOrEmpty(sessionKey))
            {
                _listener.EndSendTo(sessionKey, asyncResult);
            }
        }
    }
}
