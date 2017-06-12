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
        private ActorIdentity _centerActor;
        private ActorChannelConfiguration _channelConfiguration;
        private IActorChannel _centerChannel;

        public CenterActorDirectory(
            ActorIdentity centerActor,
            ActorChannelConfiguration channelConfiguration)
        {
            if (centerActor == null)
                throw new ArgumentNullException("centerActor");
            if (channelConfiguration == null)
                throw new ArgumentNullException("channelConfiguration");

            _centerActor = centerActor;
            _channelConfiguration = channelConfiguration;
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

        public void Activate(ActorIdentity localActor)
        {
            if (localActor == null)
                throw new ArgumentNullException("localActor");
            if (this.Active)
                throw new InvalidOperationException(
                    string.Format("Center actor [{0}] has already been activated.", _centerActor));

            _log.DebugFormat("Activating center actor [{0}].", _centerActor);
            var centerChannel = BuildActorCenterChannel(localActor);

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
                            _centerActor, retryTimes * (int)retryPeriod.TotalMilliseconds));
                }
            }
            _log.DebugFormat("Connected to center actor [{0}].", _centerActor);
            _centerChannel = centerChannel;
        }

        public void Close()
        {
            if (_centerChannel != null)
            {
                _centerChannel.Close();
                _centerChannel = null;
            }
        }

        private IActorChannel BuildActorCenterChannel(ActorIdentity localActor)
        {
            IPAddress actorCenterAddress = ResolveIPAddress(_centerActor.Address);

            int actorCenterPort = -1;
            if (!int.TryParse(_centerActor.Port, out actorCenterPort) || actorCenterPort < 0)
                throw new InvalidOperationException(string.Format(
                    "Invalid center actor port, [{0}].", _centerActor));

            var actorCenterEndPoint = new IPEndPoint(actorCenterAddress, actorCenterPort);

            var centerConnector = new ActorTransportConnector(actorCenterEndPoint, _channelConfiguration.TransportConfiguration);
            var centerChannel = new ActorConnectorReconnectableChannel(
                localActor, centerConnector, _channelConfiguration);

            return centerChannel;
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
            var actorLookupRequestData = _channelConfiguration.FrameBuilder.ControlFrameDataEncoder.EncodeFrameData(actorLookupCondition);
            var actorLookupRequest = new WhereFrame(actorLookupRequestData);
            var actorLookupRequestBuffer = _channelConfiguration.FrameBuilder.EncodeFrame(actorLookupRequest);

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
                bool isHeaderDecoded = _channelConfiguration.FrameBuilder.TryDecodeFrameHeader(
                    lookupResponseEvent.Data, lookupResponseEvent.DataOffset, lookupResponseEvent.DataLength,
                    out actorLookupResponseFrameHeader);
                if (isHeaderDecoded && actorLookupResponseFrameHeader.OpCode == OpCode.Here)
                {
                    byte[] payload;
                    int payloadOffset;
                    int payloadCount;
                    _channelConfiguration.FrameBuilder.DecodePayload(
                        lookupResponseEvent.Data, lookupResponseEvent.DataOffset, actorLookupResponseFrameHeader,
                        out payload, out payloadOffset, out payloadCount);
                    var actorLookupResponseData = _channelConfiguration.FrameBuilder.ControlFrameDataDecoder.DecodeFrameData<ActorIdentityCollection>(
                        payload, payloadOffset, payloadCount);

                    var actors = actorLookupResponseData != null ? actorLookupResponseData.Items : null;
                    if (actors != null)
                    {
                        _log.DebugFormat("Lookup actors [{0}].", actors.Count);
                        var matchedActors = matchActorFunc(actors);
                        if (matchedActors != null && matchedActors.Any())
                        {
                            _log.DebugFormat("Resolve actors [{0}].", matchedActors.Count());
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
