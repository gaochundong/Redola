using System;
using System.Collections.Generic;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class Actor
    {
        private ILog _log = Logger.Get<Actor>();
        private ActorConfiguration _configuration;
        private IActorDirectory _directory;
        private ActorChannelManager _manager;

        public Actor(ActorConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public ActorIdentity Identity { get { return _configuration.LocalActor; } }
        public ActorChannelConfiguration ChannelConfiguration { get { return _configuration.ChannelConfiguration; } }
        public string Type { get { return this.Identity.Type; } }
        public string Name { get { return this.Identity.Name; } }

        public bool Active
        {
            get
            {
                if (_manager == null)
                    return false;

                var channel = _manager.GetActorChannel(this.Identity);
                if (channel == null)
                    return false;
                else
                    return channel.Active;
            }
        }

        public void Bootup(IActorDirectory directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (this.Active)
                throw new InvalidOperationException(
                    string.Format("Local actor [{0}] has already been booted up.", this.Identity));

            _log.DebugFormat("Claim local actor [{0}].", this.Identity);

            if (_directory != null)
                throw new InvalidOperationException("Actor directory has already been assigned.");
            _directory = directory;

            _manager = new ActorChannelManager(_directory, new ActorChannelFactory(_directory, this.ChannelConfiguration));
            _manager.ChannelConnected += OnActorChannelConnected;
            _manager.ChannelDisconnected += OnActorChannelDisconnected;
            _manager.ChannelDataReceived += OnActorChannelDataReceived;

            try
            {
                _manager.ActivateLocalActor(this.Identity);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                Shutdown();
                throw new InvalidOperationException(
                    string.Format("Cannot initiate the local actor [{0}] during bootup.", this.Identity));
            }

            try
            {
                if (!_directory.Active)
                {
                    _directory.Open();
                    _directory.Register(this.Identity);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                Shutdown();
                throw new InvalidOperationException(
                    string.Format("Cannot connect to actor directory during bootup, [{0}].", this.Identity));
            }
        }

        public void Shutdown()
        {
            if (_manager != null)
            {
                _manager.CloseAllChannels();
                _manager.ChannelConnected -= OnActorChannelConnected;
                _manager.ChannelDisconnected -= OnActorChannelDisconnected;
                _manager.ChannelDataReceived -= OnActorChannelDataReceived;
                _manager = null;
            }
            if (_directory != null)
            {
                _directory.Deregister(this.Identity);
                _directory.Close();
                _directory = null;
            }
        }

        protected IEnumerable<ActorIdentity> GetAllActors()
        {
            return _manager.GetAllActors();
        }

        protected virtual void OnActorChannelConnected(object sender, ActorChannelConnectedEventArgs e)
        {
            if (Connected != null)
            {
                Connected(sender, new ActorConnectedEventArgs(e.ChannelIdentifier, e.RemoteActor));
            }
        }

        protected virtual void OnActorChannelDisconnected(object sender, ActorChannelDisconnectedEventArgs e)
        {
            if (Disconnected != null)
            {
                Disconnected(sender, new ActorDisconnectedEventArgs(e.ChannelIdentifier, e.RemoteActor));
            }
        }

        protected virtual void OnActorChannelDataReceived(object sender, ActorChannelDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(sender, new ActorDataReceivedEventArgs(e.ChannelIdentifier, e.RemoteActor, e.Data, e.DataOffset, e.DataLength));
            }
        }

        public event EventHandler<ActorConnectedEventArgs> Connected;
        public event EventHandler<ActorDisconnectedEventArgs> Disconnected;
        public event EventHandler<ActorDataReceivedEventArgs> DataReceived;

        public override string ToString()
        {
            return string.Format("{0}", this.Identity);
        }

        #region Send

        public void Send(ActorIdentity remoteActor, byte[] data)
        {
            Send(remoteActor, data, 0, data.Length);
        }

        public void Send(ActorIdentity remoteActor, byte[] data, int offset, int count)
        {
            Send(remoteActor.Type, remoteActor.Name, data, offset, count);
        }

        public void BeginSend(ActorIdentity remoteActor, byte[] data)
        {
            BeginSend(remoteActor, data, 0, data.Length);
        }

        public void BeginSend(ActorIdentity remoteActor, byte[] data, int offset, int count)
        {
            BeginSend(remoteActor.Type, remoteActor.Name, data, offset, count);
        }

        public void Send(string remoteActorType, string remoteActorName, byte[] data)
        {
            Send(remoteActorType, remoteActorName, data, 0, data.Length);
        }

        public void Send(string remoteActorType, string remoteActorName, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType, remoteActorName);
            channel.Send(channel.Identifier, data, offset, count);
        }

        public void BeginSend(string remoteActorType, string remoteActorName, byte[] data)
        {
            BeginSend(remoteActorType, remoteActorName, data, 0, data.Length);
        }

        public void BeginSend(string remoteActorType, string remoteActorName, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType, remoteActorName);
            channel.BeginSend(channel.Identifier, data, offset, count);
        }

        public void Send(string remoteActorType, byte[] data)
        {
            Send(remoteActorType, data, 0, data.Length);
        }

        public void Send(string remoteActorType, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType);
            channel.Send(channel.Identifier, data, offset, count);
        }

        public void BeginSend(string remoteActorType, byte[] data)
        {
            BeginSend(remoteActorType, data, 0, data.Length);
        }

        public void BeginSend(string remoteActorType, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType);
            channel.BeginSend(channel.Identifier, data, offset, count);
        }

        #endregion

        #region Reply

        public void Reply(string channelIdentifier, byte[] data)
        {
            Reply(channelIdentifier, data, 0, data.Length);
        }

        public void Reply(string channelIdentifier, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannelByIdentifier(channelIdentifier);
            channel.Send(channelIdentifier, data, offset, count);
        }

        public void BeginReply(string channelIdentifier, byte[] data)
        {
            BeginReply(channelIdentifier, data, 0, data.Length);
        }

        public void BeginReply(string channelIdentifier, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannelByIdentifier(channelIdentifier);
            channel.BeginSend(channelIdentifier, data, offset, count);
        }

        #endregion

        #region Broadcast

        public void Broadcast(string remoteActorType, byte[] data)
        {
            Broadcast(remoteActorType, data, 0, data.Length);
        }

        public void Broadcast(string remoteActorType, byte[] data, int offset, int count)
        {
            var channels = _manager.GetActorChannels(remoteActorType);
            foreach (var channel in channels)
            {
                channel.Send(channel.Identifier, data, offset, count);
            }
        }

        public void BeginBroadcast(string remoteActorType, byte[] data)
        {
            BeginBroadcast(remoteActorType, data, 0, data.Length);
        }

        public void BeginBroadcast(string remoteActorType, byte[] data, int offset, int count)
        {
            var channels = _manager.GetActorChannels(remoteActorType);
            foreach (var channel in channels)
            {
                channel.BeginSend(channel.Identifier, data, offset, count);
            }
        }

        public void Broadcast(IEnumerable<string> remoteActorTypes, byte[] data)
        {
            Broadcast(remoteActorTypes, data, 0, data.Length);
        }

        public void Broadcast(IEnumerable<string> remoteActorTypes, byte[] data, int offset, int count)
        {
            foreach (var remoteActorType in remoteActorTypes)
            {
                Broadcast(remoteActorType, data, offset, count);
            }
        }

        public void BeginBroadcast(IEnumerable<string> remoteActorTypes, byte[] data)
        {
            BeginBroadcast(remoteActorTypes, data, 0, data.Length);
        }

        public void BeginBroadcast(IEnumerable<string> remoteActorTypes, byte[] data, int offset, int count)
        {
            foreach (var remoteActorType in remoteActorTypes)
            {
                BeginBroadcast(remoteActorType, data, offset, count);
            }
        }

        #endregion
    }
}
