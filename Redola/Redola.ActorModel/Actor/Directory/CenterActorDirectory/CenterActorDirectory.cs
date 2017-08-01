using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class CenterActorDirectory : IActorDirectory
    {
        private ILog _log = Logger.Get<CenterActorDirectory>();
        private CenterActorDirectoryConfiguration _configuration;
        private IActorChannel _centerChannel;
        private readonly object _openLock = new object();

        public CenterActorDirectory(CenterActorDirectoryConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public ActorIdentity CenterActor { get { return _configuration.CenterActor; } }
        public ActorChannelConfiguration ChannelConfiguration { get { return _configuration.ChannelConfiguration; } }

        public void Open()
        {
        }

        public void Close()
        {
            lock (_openLock)
            {
                if (_centerChannel != null)
                {
                    _centerChannel.Close();
                    _centerChannel.ChannelDataReceived -= OnCenterChannelDataReceived;
                    _centerChannel = null;
                }
            }
        }

        public bool Active
        {
            get
            {
                if (_centerChannel == null)
                    return false;

                return _centerChannel.Active;
            }
        }

        public void Register(ActorIdentity localActor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");

            lock (_openLock)
            {
                if (this.Active)
                    throw new InvalidOperationException(
                        string.Format("Center actor [{0}] has already been registered.", this.CenterActor));

                _log.DebugFormat("Connecting to center actor [{0}].", this.CenterActor);
                var centerChannel = BuildCenterActorChannel(localActor);

                centerChannel.Open();
                int retryTimes = 1;
                TimeSpan retryPeriod = TimeSpan.FromMilliseconds(100);
                while (true)
                {
                    if (centerChannel.Active)
                        break;

                    Thread.Sleep(retryPeriod);

                    if (centerChannel.Active)
                        break;

                    retryTimes++;
                    if (retryTimes > 300)
                    {
                        centerChannel.Close();
                        throw new InvalidOperationException(
                            string.Format("Cannot connect to center actor [{0}] after wait [{1}] milliseconds.",
                                this.CenterActor, retryTimes * (int)retryPeriod.TotalMilliseconds));
                    }
                }
                _log.DebugFormat("Connected to center actor [{0}].", this.CenterActor);
                _centerChannel = centerChannel;
                _centerChannel.ChannelDataReceived += OnCenterChannelDataReceived;
            }
        }

        public void Deregister(ActorIdentity localActor)
        {
        }

        private IActorChannel BuildCenterActorChannel(ActorIdentity localActor)
        {
            IPAddress centerActorAddress = ResolveIPAddress(this.CenterActor.Address);

            int centerActorPort = -1;
            if (!int.TryParse(this.CenterActor.Port, out centerActorPort) || centerActorPort < 0)
                throw new InvalidOperationException(string.Format(
                    "Invalid center actor port, [{0}].", this.CenterActor));

            var centerActorEndPoint = new IPEndPoint(centerActorAddress, centerActorPort);

            var centerConnector = new ActorTransportConnector(centerActorEndPoint, this.ChannelConfiguration.TransportConfiguration);
            var centerChannel = new ActorConnectorReconnectableChannel(
                localActor, centerConnector, this.ChannelConfiguration);

            return centerChannel;
        }

        private void OnCenterChannelDataReceived(object sender, ActorChannelDataReceivedEventArgs e)
        {
            ActorFrameHeader actorChangeNotificationFrameHeader = null;
            bool isHeaderDecoded = this.ChannelConfiguration.FrameBuilder.TryDecodeFrameHeader(
                e.Data, e.DataOffset, e.DataLength,
                out actorChangeNotificationFrameHeader);
            if (isHeaderDecoded && actorChangeNotificationFrameHeader.OpCode == OpCode.Change)
            {
                byte[] payload;
                int payloadOffset;
                int payloadCount;
                this.ChannelConfiguration.FrameBuilder.DecodePayload(
                    e.Data, e.DataOffset, actorChangeNotificationFrameHeader,
                    out payload, out payloadOffset, out payloadCount);
                var actorChangeNotificationData = this.ChannelConfiguration.FrameBuilder.ControlFrameDataDecoder.DecodeFrameData<ActorIdentityCollection>(
                    payload, payloadOffset, payloadCount);

                var actors = actorChangeNotificationData != null ? actorChangeNotificationData.Items : null;
                if (actors != null && actors.Any())
                {
                    _log.DebugFormat("Actor changed, ActorType[{0}], AvailableCount[{1}].", actors.First().Type, actors.Count);
                    RaiseActorsChanged(actors);
                }
            }
        }

        public IPEndPoint LookupRemoteActorEndPoint(string actorType, string actorName)
        {
            if (string.IsNullOrEmpty(actorType))
                throw new ArgumentNullException("actorType");
            if (string.IsNullOrEmpty(actorName))
                throw new ArgumentNullException("actorName");

            var endpoints = LookupRemoteActorEndPoints(
                actorType,
                (actors) =>
                {
                    return actors.Where(a => a.Type == actorType && a.Name == actorName);
                });

            if (endpoints == null || endpoints.Count() == 0)
                throw new ActorNotFoundException(string.Format(
                    "Cannot lookup remote actor, Type[{0}], Name[{1}].", actorType, actorName));

            if (endpoints.Count() > 1)
                throw new ActorNotFoundException(string.Format(
                    "Duplicate remote actor found, Type[{0}], Name[{1}].", actorType, actorName));

            return endpoints.Single();
        }

        public IEnumerable<IPEndPoint> LookupRemoteActorEndPoints(string actorType)
        {
            if (string.IsNullOrEmpty(actorType))
                throw new ArgumentNullException("actorType");

            var endpoints = LookupRemoteActorEndPoints(
                actorType,
                (actors) =>
                {
                    return actors.Where(a => a.Type == actorType);
                });

            if (endpoints == null || !endpoints.Any())
                throw new ActorNotFoundException(string.Format(
                    "Cannot lookup remote actor, Type[{0}].", actorType));

            return endpoints;
        }

        public IEnumerable<ActorIdentity> LookupRemoteActors(string actorType)
        {
            if (string.IsNullOrEmpty(actorType))
                throw new ArgumentNullException("actorType");

            var remoteActors = LookupRemoteActors(
                actorType,
                (actors) =>
                {
                    return actors.Where(a => a.Type == actorType);
                });

            if (remoteActors == null || !remoteActors.Any())
                throw new ActorNotFoundException(string.Format(
                    "Cannot lookup remote actor, Type[{0}].", actorType));

            return remoteActors;
        }

        private IEnumerable<IPEndPoint> LookupRemoteActorEndPoints(string actorType, Func<IEnumerable<ActorIdentity>, IEnumerable<ActorIdentity>> matchActorFunc)
        {
            var remoteActors = LookupRemoteActors(actorType, matchActorFunc);

            if (remoteActors != null && remoteActors.Any())
            {
                return remoteActors.Select(a => ConvertActorToEndPoint(a));
            }

            return null;
        }

        private IEnumerable<ActorIdentity> LookupRemoteActors(string actorType, Func<IEnumerable<ActorIdentity>, IEnumerable<ActorIdentity>> matchActorFunc)
        {
            var actorLookupCondition = new ActorIdentityLookup()
            {
                Type = actorType,
            };
            var actorLookupRequestData = this.ChannelConfiguration.FrameBuilder.ControlFrameDataEncoder.EncodeFrameData(actorLookupCondition);
            var actorLookupRequest = new WhereFrame(actorLookupRequestData);
            var actorLookupRequestBuffer = this.ChannelConfiguration.FrameBuilder.EncodeFrame(actorLookupRequest);

            ManualResetEventSlim waitingResponse = new ManualResetEventSlim(false);
            ActorChannelDataReceivedEventArgs lookupResponseEvent = null;
            EventHandler<ActorChannelDataReceivedEventArgs> onDataReceived =
                (s, e) =>
                {
                    lookupResponseEvent = e;
                    waitingResponse.Set();
                };

            _centerChannel.ChannelDataReceived += onDataReceived;
            _centerChannel.BeginSend(_centerChannel.Identifier, actorLookupRequestBuffer);

            bool lookedup = waitingResponse.Wait(TimeSpan.FromSeconds(15));
            _centerChannel.ChannelDataReceived -= onDataReceived;
            waitingResponse.Dispose();

            if (lookedup && lookupResponseEvent != null)
            {
                ActorFrameHeader actorLookupResponseFrameHeader = null;
                bool isHeaderDecoded = this.ChannelConfiguration.FrameBuilder.TryDecodeFrameHeader(
                    lookupResponseEvent.Data, lookupResponseEvent.DataOffset, lookupResponseEvent.DataLength,
                    out actorLookupResponseFrameHeader);
                if (isHeaderDecoded && actorLookupResponseFrameHeader.OpCode == OpCode.Here)
                {
                    byte[] payload;
                    int payloadOffset;
                    int payloadCount;
                    this.ChannelConfiguration.FrameBuilder.DecodePayload(
                        lookupResponseEvent.Data, lookupResponseEvent.DataOffset, actorLookupResponseFrameHeader,
                        out payload, out payloadOffset, out payloadCount);
                    var actorLookupResponseData = this.ChannelConfiguration.FrameBuilder.ControlFrameDataDecoder.DecodeFrameData<ActorIdentityCollection>(
                        payload, payloadOffset, payloadCount);

                    var actors = actorLookupResponseData != null ? actorLookupResponseData.Items : null;
                    if (actors != null)
                    {
                        _log.DebugFormat("Lookup actors, ActorType[{0}], Count[{1}].", actorType, actors.Count);
                        var matchedActors = matchActorFunc(actors);
                        if (matchedActors != null && matchedActors.Any())
                        {
                            _log.DebugFormat("Resolve actors, ActorType[{0}], Count[{1}].", actorType, matchedActors.Count());
                            return matchedActors;
                        }
                    }
                }
            }

            return null;
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
                            string.Format("Cannot resolve host [{0}] from DNS.", host));
                    }
                }
            }

            return remoteIPAddress;
        }

        private IPEndPoint ConvertActorToEndPoint(ActorIdentity actor)
        {
            var actorAddress = ResolveIPAddress(actor.Address);
            int actorPort = int.Parse(actor.Port);
            return new IPEndPoint(actorAddress, actorPort);
        }

        public event EventHandler<ActorsChangedEventArgs> ActorsChanged;

        private void RaiseActorsChanged(IEnumerable<ActorIdentity> actors)
        {
            if (ActorsChanged != null)
            {
                ActorsChanged(this, new ActorsChangedEventArgs(actors));
            }
        }
    }
}
