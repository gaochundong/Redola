using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Logrila.Logging;
using Redola.ActorModel.Extensions;

namespace Redola.ActorModel
{
    public class ActorChannelManager
    {
        private ILog _log = Logger.Get<ActorChannelManager>();
        private ActorIdentity _localActor;
        private ActorChannelFactory _factory;
        private ConcurrentDictionary<string, IActorChannel> _channels = new ConcurrentDictionary<string, IActorChannel>(); // ActorKey -> IActorChannel
        private ConcurrentDictionary<string, ActorIdentity> _actorKeys = new ConcurrentDictionary<string, ActorIdentity>(); // ActorKey -> ActorIdentity
        private readonly object _syncLock = new object();

        public ActorChannelManager(ActorChannelFactory factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            _factory = factory;
        }

        public void ActivateLocalActor(ActorIdentity localActor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            if (_localActor != null)
                throw new InvalidOperationException("The local actor has already been activated.");

            var channel = _factory.BuildLocalActor(localActor);
            channel.ChannelConnected += OnActorChannelConnected;
            channel.ChannelDisconnected += OnActorChannelDisconnected;
            channel.ChannelDataReceived += OnActorChannelDataReceived;

            try
            {
                channel.Open();

                _localActor = localActor;
                _channels.Add(_localActor.GetKey(), channel);
                _actorKeys.Add(_localActor.GetKey(), _localActor);

                _log.DebugFormat("Local actor [{0}] is activated.", _localActor);
            }
            catch
            {
                CloseChannel(channel);
                throw;
            }
        }

        public IActorChannel GetActorChannel(ActorIdentity remoteActor)
        {
            if (remoteActor == null)
                throw new ArgumentNullException("remoteActor");
            return GetActorChannel(remoteActor.Type, remoteActor.Name);
        }

        public IActorChannel GetActorChannel(string actorType, string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return GetActorChannel(actorType);

            IActorChannel channel = null;
            var actorKey = ActorIdentity.GetKey(actorType, actorName);

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
                bool activated = ActivateChannel(channel);
                if (activated)
                {
                    return channel;
                }
                else
                {
                    throw new ActorNotFoundException(string.Format(
                        "Activate channel failed, cannot connect remote actor, Type[{0}], Name[{1}].", actorType, actorName));
                }
            }
        }

        public IActorChannel GetActorChannel(string actorType)
        {
            if (string.IsNullOrEmpty(actorType))
                throw new ArgumentNullException("actorType");

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
                bool activated = ActivateChannel(channel);
                if (activated)
                {
                    return channel;
                }
                else
                {
                    throw new ActorNotFoundException(string.Format(
                        "Activate channel failed, cannot connect remote actor, Type[{0}].", actorType));
                }
            }
        }

        private bool ActivateChannel(IActorChannel channel)
        {
            channel.ChannelConnected += OnActorChannelConnected;
            channel.ChannelDisconnected += OnActorChannelDisconnected;
            channel.ChannelDataReceived += OnActorChannelDataReceived;

            ManualResetEventSlim waitingConnected = new ManualResetEventSlim(false);
            object connectedSender = null;
            ActorChannelConnectedEventArgs connectedEvent = null;
            EventHandler<ActorChannelConnectedEventArgs> onConnected =
                (s, e) =>
                {
                    connectedSender = s;
                    connectedEvent = e;
                    waitingConnected.Set();
                };

            channel.ChannelConnected += onConnected;
            channel.Open();

            bool connected = waitingConnected.Wait(TimeSpan.FromSeconds(5));
            channel.ChannelConnected -= onConnected;
            waitingConnected.Dispose();

            if (connected && channel.Active)
            {
                _channels.Add(connectedEvent.RemoteActor.GetKey(), (IActorChannel)connectedSender);
                _actorKeys.Add(connectedEvent.RemoteActor.GetKey(), connectedEvent.RemoteActor);
                return true;
            }
            else
            {
                CloseChannel(channel);
                return false;
            }
        }

        public IEnumerable<IActorChannel> GetActorChannels(string actorType)
        {
            if (string.IsNullOrEmpty(actorType))
                throw new ArgumentNullException("actorType");
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
            channel.ChannelConnected -= OnActorChannelConnected;
            channel.ChannelDisconnected -= OnActorChannelDisconnected;
            channel.ChannelDataReceived -= OnActorChannelDataReceived;
            channel.Close();
        }

        private void OnActorChannelConnected(object sender, ActorChannelConnectedEventArgs e)
        {
            lock (_syncLock)
            {
                var remoteActorKey = e.RemoteActor.GetKey();
                var channel = _channels.Get(remoteActorKey);
                if (channel != null)
                {
                    if (channel.Identifier != e.ActorChannelIdentifier)
                    {
                        var removedChannel = _channels.Remove(remoteActorKey);
                        var removedActorIdentity = _actorKeys.Remove(remoteActorKey);

                        CloseChannel(channel);

                        if (ChannelDisconnected != null)
                        {
                            ChannelDisconnected(sender, new ActorChannelDisconnectedEventArgs(removedChannel.Identifier, removedActorIdentity));
                        }
                    }
                }

                _channels.Add(remoteActorKey, (IActorChannel)sender);
                _actorKeys.Add(remoteActorKey, e.RemoteActor);

                if (ChannelConnected != null)
                {
                    ChannelConnected(sender, e);
                }
            }
        }

        private void OnActorChannelDisconnected(object sender, ActorChannelDisconnectedEventArgs e)
        {
            lock (_syncLock)
            {
                var remoteActorKey = e.RemoteActor.GetKey();
                var channel = _channels.Get(remoteActorKey);
                if (channel != null)
                {
                    if (channel.Identifier == e.ActorChannelIdentifier)
                    {
                        var removedChannel = _channels.Remove(remoteActorKey);
                        var removedActorIdentity = _actorKeys.Remove(remoteActorKey);

                        if (ChannelDisconnected != null)
                        {
                            ChannelDisconnected(sender, new ActorChannelDisconnectedEventArgs(removedChannel.Identifier, removedActorIdentity));
                        }
                    }
                }
            }
        }

        private void OnActorChannelDataReceived(object sender, ActorChannelDataReceivedEventArgs e)
        {
            if (ChannelDataReceived != null)
            {
                ChannelDataReceived(sender, e);
            }
        }

        public event EventHandler<ActorChannelConnectedEventArgs> ChannelConnected;
        public event EventHandler<ActorChannelDisconnectedEventArgs> ChannelDisconnected;
        public event EventHandler<ActorChannelDataReceivedEventArgs> ChannelDataReceived;

        public List<ActorIdentity> GetAllActors()
        {
            return _actorKeys.Values.ToList();
        }
    }
}
