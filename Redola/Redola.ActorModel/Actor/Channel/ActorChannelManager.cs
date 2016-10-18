using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redola.ActorModel.Extensions;

namespace Redola.ActorModel
{
    public class ActorChannelManager
    {
        private ActorDescription _localActor;
        private ActorChannelFactory _factory;
        private ConcurrentDictionary<string, IActorChannel> _channels = new ConcurrentDictionary<string, IActorChannel>(); // ActorKey -> IActorChannel
        private ConcurrentDictionary<string, ActorDescription> _actorKeys = new ConcurrentDictionary<string, ActorDescription>(); // ActorKey -> ActorDescription
        private readonly object _syncLock = new object();

        public ActorChannelManager(ActorChannelFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _factory = factory;
        }

        public void ActivateLocalActor(ActorDescription localActor)
        {
            var channel = _factory.BuildLocalActor(localActor);
            channel.Connected += OnActorConnected;
            channel.Disconnected += OnActorDisconnected;
            channel.DataReceived += OnActorDataReceived;
            channel.Open();

            _localActor = localActor;
            _channels.Add(_localActor.GetKey(), channel);
            _actorKeys.Add(_localActor.GetKey(), _localActor);
        }

        public IActorChannel GetActorChannel(ActorDescription remoteActor)
        {
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");
            return GetActorChannel(remoteActor.Type, remoteActor.Name);
        }

        public IActorChannel GetActorChannel(string actorType, string actorName)
        {
            IActorChannel channel = null;
            var actorKey = ActorDescription.GetKey(actorType, actorName);

            if (_channels.TryGetValue(actorKey, out channel))
            {
                return channel;
            }

            lock (_syncLock)
            {
                if (_channels.TryGetValue(actorKey, out channel))
                {
                    return channel;
                }

                channel = _factory.BuildActorChannel(_localActor, actorType, actorName);
                channel.Connected += OnActorConnected;
                channel.Disconnected += OnActorDisconnected;
                channel.DataReceived += OnActorDataReceived;

                ManualResetEventSlim waitingConnected = new ManualResetEventSlim(false);
                object connectedSender = null;
                ActorConnectedEventArgs connectedEvent = null;
                EventHandler<ActorConnectedEventArgs> onConnected =
                    (s, e) =>
                    {
                        connectedSender = s;
                        connectedEvent = e;
                        waitingConnected.Set();
                    };

                channel.Connected += onConnected;
                channel.Open();

                bool connected = waitingConnected.Wait(TimeSpan.FromSeconds(5));
                channel.Connected -= onConnected;
                waitingConnected.Dispose();

                if (connected)
                {
                    _channels.Add(connectedEvent.RemoteActor.GetKey(), (IActorChannel)connectedSender);
                    _actorKeys.Add(connectedEvent.RemoteActor.GetKey(), connectedEvent.RemoteActor);
                    return channel;
                }
                else
                {
                    CloseChannel(channel);
                    throw new ActorNotFoundException(string.Format(
                        "Cannot connect remote actor, Type[{0}], Name[{1}].", actorType, actorName));
                }
            }
        }

        public IActorChannel GetActorChannel(string actorType)
        {
            IActorChannel channel = null;
            var actor = _actorKeys.Values.Where(a => a.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();

            if (actor != null && _channels.TryGetValue(actor.GetKey(), out channel))
            {
                return channel;
            }

            lock (_syncLock)
            {
                actor = _actorKeys.Values.Where(a => a.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();

                if (actor != null && _channels.TryGetValue(actor.GetKey(), out channel))
                {
                    return channel;
                }

                channel = _factory.BuildActorChannel(_localActor, actorType);
                channel.Connected += OnActorConnected;
                channel.Disconnected += OnActorDisconnected;
                channel.DataReceived += OnActorDataReceived;

                ManualResetEventSlim waitingConnected = new ManualResetEventSlim(false);
                object connectedSender = null;
                ActorConnectedEventArgs connectedEvent = null;
                EventHandler<ActorConnectedEventArgs> onConnected =
                    (s, e) =>
                    {
                        connectedSender = s;
                        connectedEvent = e;
                        waitingConnected.Set();
                    };

                channel.Connected += onConnected;
                channel.Open();

                bool connected = waitingConnected.Wait(TimeSpan.FromSeconds(5));
                channel.Connected -= onConnected;
                waitingConnected.Dispose();

                if (connected)
                {
                    _channels.Add(connectedEvent.RemoteActor.GetKey(), (IActorChannel)connectedSender);
                    _actorKeys.Add(connectedEvent.RemoteActor.GetKey(), connectedEvent.RemoteActor);
                    return channel;
                }
                else
                {
                    CloseChannel(channel);
                    throw new ActorNotFoundException(string.Format(
                        "Cannot connect remote actor, Type[{0}].", actorType));
                }
            }
        }

        public IEnumerable<IActorChannel> GetActorChannels(string actorType)
        {
            return _actorKeys.Values.Where(a => a.Type == actorType).Select(v => _channels.Get(v.GetKey()));
        }

        public void CloseAllChannels()
        {
            foreach (var channel in _channels)
            {
                CloseChannel(channel.Value);
                _channels.Remove(channel.Key);
                _actorKeys.Remove(channel.Key);
            }
        }

        private void CloseChannel(IActorChannel channel)
        {
            channel.Connected -= OnActorConnected;
            channel.Disconnected -= OnActorDisconnected;
            channel.DataReceived -= OnActorDataReceived;
            channel.Close();
        }

        private void OnActorConnected(object sender, ActorConnectedEventArgs e)
        {
            _channels.TryAdd(e.RemoteActor.GetKey(), (IActorChannel)sender);
            _actorKeys.TryAdd(e.RemoteActor.GetKey(), e.RemoteActor);

            if (Connected != null)
            {
                Connected(sender, e);
            }
        }

        private void OnActorDisconnected(object sender, ActorDisconnectedEventArgs e)
        {
            _channels.Remove(e.RemoteActor.GetKey());
            _actorKeys.Remove(e.RemoteActor.GetKey());

            if (Disconnected != null)
            {
                Disconnected(sender, e);
            }
        }

        private void OnActorDataReceived(object sender, ActorDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(sender, e);
            }
        }

        public event EventHandler<ActorConnectedEventArgs> Connected;
        public event EventHandler<ActorDisconnectedEventArgs> Disconnected;
        public event EventHandler<ActorDataReceivedEventArgs> DataReceived;

        public List<ActorDescription> GetAllActors()
        {
            return _actorKeys.Values.ToList();
        }
    }
}
