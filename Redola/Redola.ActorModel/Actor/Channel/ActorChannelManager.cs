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

        private class ChannelItem
        {
            public ChannelItem() { }
            public ChannelItem(string channelIdentifier, IActorChannel channel)
            {
                this.ChannelIdentifier = channelIdentifier;
                this.Channel = channel;
            }

            public string ChannelIdentifier { get; set; }
            public IActorChannel Channel { get; set; }

            public string RemoteActorKey { get; set; }
            public ActorIdentity RemoteActor { get; set; }

            public override string ToString()
            {
                return string.Format("{0}#{1}", ChannelIdentifier, RemoteActor);
            }
        }
        private ConcurrentDictionary<string, ChannelItem> _channels
            = new ConcurrentDictionary<string, ChannelItem>(); // ChannelIdentifier -> ChannelItem
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

                var item = new ChannelItem(channel.Identifier, channel);
                item.RemoteActorKey = _localActor.GetKey();
                item.RemoteActor = _localActor;
                _channels.Add(channel.Identifier, item);

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
            {
                return GetActorChannel(actorType);
            }

            var actorKey = ActorIdentity.GetKey(actorType, actorName);
            ChannelItem item = null;

            item = _channels.Values.FirstOrDefault(i => i.RemoteActorKey == actorKey);
            if (item != null)
            {
                return item.Channel;
            }

            lock (_syncLock)
            {
                item = _channels.Values.FirstOrDefault(i => i.RemoteActorKey == actorKey);
                if (item != null)
                {
                    return item.Channel;
                }

                var channel = _factory.BuildActorChannel(_localActor, actorType, actorName);
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

            ChannelItem item = null;

            item = _channels.Values.Where(i => i.RemoteActor.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
            if (item != null)
            {
                return item.Channel;
            }

            lock (_syncLock)
            {
                item = _channels.Values.Where(i => i.RemoteActor.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
                if (item != null)
                {
                    return item.Channel;
                }

                var channel = _factory.BuildActorChannel(_localActor, actorType);
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
                var item = new ChannelItem(((IActorChannel)connectedSender).Identifier, (IActorChannel)connectedSender);
                item.RemoteActorKey = connectedEvent.RemoteActor.GetKey();
                item.RemoteActor = connectedEvent.RemoteActor;
                _channels.TryAdd(channel.Identifier, item);
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
            return _channels.Values.Where(i => i.RemoteActor.Type == actorType).Select(v => v.Channel);
        }

        public void CloseAllChannels()
        {
            foreach (var item in _channels.Values)
            {
                CloseChannel(item.Channel);
                _channels.Remove(item.ChannelIdentifier);
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
            var item = _channels.Get(e.ChannelIdentifier);
            if (item != null)
            {
                if (item.RemoteActorKey != e.RemoteActor.GetKey())
                {
                    _channels.Remove(e.ChannelIdentifier);
                    CloseChannel(item.Channel);

                    if (item.RemoteActor != null)
                    {
                        if (ChannelDisconnected != null)
                        {
                            ChannelDisconnected(sender, new ActorChannelDisconnectedEventArgs(item.ChannelIdentifier, item.RemoteActor));
                        }
                    }
                }
                else
                {
                    return;
                }
            }

            item = new ChannelItem(((IActorChannel)sender).Identifier, (IActorChannel)sender);
            item.RemoteActorKey = e.RemoteActor.GetKey();
            item.RemoteActor = e.RemoteActor;
            _channels.TryAdd(item.ChannelIdentifier, item);

            if (ChannelConnected != null)
            {
                ChannelConnected(sender, e);
            }
        }

        private void OnActorChannelDisconnected(object sender, ActorChannelDisconnectedEventArgs e)
        {
            var item = _channels.Get(e.ChannelIdentifier);
            if (item != null)
            {
                if (item.RemoteActorKey == e.RemoteActor.GetKey())
                {
                    _channels.Remove(e.ChannelIdentifier);
                    CloseChannel(item.Channel);

                    if (item.RemoteActor != null)
                    {
                        if (ChannelDisconnected != null)
                        {
                            ChannelDisconnected(sender, new ActorChannelDisconnectedEventArgs(item.ChannelIdentifier, item.RemoteActor));
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
            return _channels.Values.Select(c => c.RemoteActor).Where(f => f != null).ToList();
        }
    }
}
