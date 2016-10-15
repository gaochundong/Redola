using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class Actor
    {
        private ILog _log = Logger.Get<Actor>();
        private ActorConfiguration _configuration;
        private ActorDirectory _directory;
        private ActorChannelFactory _factory;
        private ActorChannelManager _manager;

        public Actor(ActorConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public IActorMessageEncoder Encoder { get { return _configuration.Encoder; } }
        public IActorMessageDecoder Decoder { get { return _configuration.Decoder; } }
        public ActorDescription CenterActor { get { return _configuration.CenterActor; } }
        public ActorDescription LocalActor { get { return _configuration.LocalActor; } }
        public string Type { get { return this.LocalActor.Type; } }
        public string Name { get { return this.LocalActor.Name; } }

        public bool Active
        {
            get
            {
                var channel = _manager.GetActorChannel(this.LocalActor);
                if (channel == null)
                    return false;
                else
                    return channel.Active;
            }
        }

        public void Bootup()
        {
            _log.InfoFormat("Claim local actor [{0}].", this.LocalActor);
            _log.InfoFormat("Register center actor [{0}].", this.CenterActor);

            var centerChannel = BuildActorCenterChannel(this.CenterActor, this.LocalActor);
            _directory = new ActorDirectory(this.CenterActor, centerChannel, this.Encoder, this.Decoder);
            _factory = new ActorChannelFactory(_directory, this.Encoder, this.Decoder);
            _manager = new ActorChannelManager(_factory);
            _manager.Connected += OnActorConnected;
            _manager.Disconnected += OnActorDisconnected;
            _manager.DataReceived += OnActorDataReceived;

            _manager.ActivateLocalActor(this.LocalActor);
            centerChannel.Open();

            int retryTimes = 0;
            while (true)
            {
                if (centerChannel.Active) break;

                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
                retryTimes++;
                if (retryTimes > 300)
                {
                    Shutdown();
                    throw new InvalidOperationException("Cannot connect to center actor.");
                }
            }
        }

        public void Shutdown()
        {
            _manager.Connected -= OnActorConnected;
            _manager.Disconnected -= OnActorDisconnected;
            _manager.DataReceived -= OnActorDataReceived;
            _manager.CloseAllChannels();
            _directory.GetCenterActorChannel().Close();
        }

        private IActorChannel BuildActorCenterChannel(ActorDescription centerActor, ActorDescription localActor)
        {
            IPAddress actorCenterAddress = ResolveIPAddress(centerActor.Address);
            int actorCenterPort = int.Parse(centerActor.Port);
            var actorCenterEndPoint = new IPEndPoint(actorCenterAddress, actorCenterPort);

            var centerConnector = new ActorTransportConnector(actorCenterEndPoint);
            var centerChannel = new ActorConnectorReconnectableChannel(localActor, centerConnector, this.Encoder, this.Decoder);

            return centerChannel;
        }

        protected List<ActorDescription> GetAllActors()
        {
            return _manager.GetAllActors();
        }

        protected virtual void OnActorConnected(object sender, ActorConnectedEventArgs e)
        {
            if (Connected != null)
            {
                Connected(sender, e);
            }
        }

        protected virtual void OnActorDisconnected(object sender, ActorDisconnectedEventArgs e)
        {
            if (Disconnected != null)
            {
                Disconnected(sender, e);
            }
        }

        protected virtual void OnActorDataReceived(object sender, ActorDataReceivedEventArgs e)
        {
            if (DataReceived != null)
            {
                DataReceived(sender, e);
            }
        }

        public event EventHandler<ActorConnectedEventArgs> Connected;
        public event EventHandler<ActorDisconnectedEventArgs> Disconnected;
        public event EventHandler<ActorDataReceivedEventArgs> DataReceived;

        public void Send(ActorDescription remoteActor, byte[] data)
        {
            var channel = _manager.GetActorChannel(remoteActor);
            channel.Send(remoteActor.Type, remoteActor.Name, data);
        }

        public void Send(ActorDescription remoteActor, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActor);
            channel.Send(remoteActor.Type, remoteActor.Name, data, offset, count);
        }

        public void SendAsync(ActorDescription remoteActor, byte[] data)
        {
            var channel = _manager.GetActorChannel(remoteActor);
            channel.BeginSend(remoteActor.Type, remoteActor.Name, data);
        }

        public void SendAsync(ActorDescription remoteActor, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActor);
            channel.BeginSend(remoteActor.Type, remoteActor.Name, data, offset, count);
        }

        public void Send(string remoteActorType, string remoteActorName, byte[] data)
        {
            var channel = _manager.GetActorChannel(remoteActorType, remoteActorName);
            channel.Send(remoteActorType, remoteActorName, data);
        }

        public void Send(string remoteActorType, string remoteActorName, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType, remoteActorName);
            channel.Send(remoteActorType, remoteActorName, data, offset, count);
        }

        public void SendAsync(string remoteActorType, string remoteActorName, byte[] data)
        {
            var channel = _manager.GetActorChannel(remoteActorType, remoteActorName);
            channel.BeginSend(remoteActorType, remoteActorName, data);
        }

        public void SendAsync(string remoteActorType, string remoteActorName, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType, remoteActorName);
            channel.BeginSend(remoteActorType, remoteActorName, data, offset, count);
        }

        public void Send(string remoteActorType, byte[] data)
        {
            var channel = _manager.GetActorChannel(remoteActorType);
            channel.Send(remoteActorType, data);
        }

        public void Send(string remoteActorType, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType);
            channel.Send(remoteActorType, data, offset, count);
        }

        public void SendAsync(string remoteActorType, byte[] data)
        {
            var channel = _manager.GetActorChannel(remoteActorType);
            channel.BeginSend(remoteActorType, data);
        }

        public void SendAsync(string remoteActorType, byte[] data, int offset, int count)
        {
            var channel = _manager.GetActorChannel(remoteActorType);
            channel.BeginSend(remoteActorType, data, offset, count);
        }

        public void Broadcast(string remoteActorType, byte[] data)
        {
            var channels = _manager.GetActorChannels(remoteActorType);
            foreach (var channel in channels.Where(c => c != null))
            {
                channel.Send(remoteActorType, data);
            }
        }

        public void Broadcast(string remoteActorType, byte[] data, int offset, int count)
        {
            var channels = _manager.GetActorChannels(remoteActorType);
            foreach (var channel in channels.Where(c => c != null))
            {
                channel.Send(remoteActorType, data, offset, count);
            }
        }

        public void BroadcastAsync(string remoteActorType, byte[] data)
        {
            var channels = _manager.GetActorChannels(remoteActorType);
            foreach (var channel in channels.Where(c => c != null))
            {
                channel.BeginSend(remoteActorType, data);
            }
        }

        public void BroadcastAsync(string remoteActorType, byte[] data, int offset, int count)
        {
            var channels = _manager.GetActorChannels(remoteActorType);
            foreach (var channel in channels.Where(c => c != null))
            {
                channel.BeginSend(remoteActorType, data, offset, count);
            }
        }

        private IPAddress ResolveIPAddress(string host)
        {
            IPAddress remoteIPAddress = null;

            IPAddress ipAddress;
            if (IPAddress.TryParse(host, out ipAddress))
            {
                remoteIPAddress = ipAddress;
            }
            else
            {
                if (host.ToLowerInvariant() == "localhost")
                {
                    remoteIPAddress = IPAddress.Parse(@"127.0.0.1");
                }
                else
                {
                    IPAddress[] addresses = Dns.GetHostAddresses(host);
                    if (addresses.Any())
                    {
                        remoteIPAddress = addresses.First();
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format("Cannot resolve host [{0}] by DNS.", host));
                    }
                }
            }

            return remoteIPAddress;
        }

        public override string ToString()
        {
            return string.Format("{0}", this.LocalActor);
        }
    }
}
