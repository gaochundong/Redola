using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Logrila.Logging;
using Redola.ActorModel.Framing;

namespace Redola.ActorModel
{
    public class ActorDirectory
    {
        private ILog _log = Logger.Get<ActorDirectory>();
        private ActorDescription _centerActor;
        private IActorChannel _centerChannel;
        private IActorFrameBuilder _frameBuilder;

        public ActorDirectory(
            ActorDescription centerActor,
            IActorChannel centerChannel,
            IActorFrameBuilder frameBuilder)
        {
            if (centerActor == null)
                throw new ArgumentNullException("centerActor");
            if (centerChannel == null)
                throw new ArgumentNullException("centerChannel");
            if (frameBuilder == null)
                throw new ArgumentNullException("frameBuilder");

            _centerActor = centerActor;
            _centerChannel = centerChannel;
            _frameBuilder = frameBuilder;
        }

        public ActorDescription GetCenterActor()
        {
            return _centerActor;
        }

        public IActorChannel GetCenterActorChannel()
        {
            return _centerChannel;
        }

        public IPEndPoint LookupRemoteActorEndPoint(string actorType, string actorName)
        {
            var endpoint = LookupRemoteActorEndPoint(
                (actors) =>
                {
                    return actors.FirstOrDefault(a => a.Type == actorType && a.Name == actorName);
                });

            if (endpoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot lookup remote actor, Type[{0}], Name[{1}].", actorType, actorName));

            return endpoint;
        }

        public IPEndPoint LookupRemoteActorEndPoint(string actorType)
        {
            var endpoint = LookupRemoteActorEndPoint(
                (actors) =>
                {
                    return actors.Where(a => a.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
                });

            if (endpoint == null)
                throw new ActorNotFoundException(string.Format(
                    "Cannot lookup remote actor, Type[{0}].", actorType));

            return endpoint;
        }

        private IPEndPoint LookupRemoteActorEndPoint(Func<List<ActorDescription>, ActorDescription> matchActorFunc)
        {
            var actorLookupRequest = new WhereFrame();
            var actorLookupRequestBuffer = _frameBuilder.EncodeFrame(actorLookupRequest);

            ManualResetEventSlim waitingResponse = new ManualResetEventSlim(false);
            ActorDataReceivedEventArgs lookupResponseEvent = null;
            EventHandler<ActorDataReceivedEventArgs> onDataReceived =
                (s, e) =>
                {
                    lookupResponseEvent = e;
                    waitingResponse.Set();
                };

            _centerChannel.DataReceived += onDataReceived;
            _centerChannel.BeginSend(_centerActor.Type, _centerActor.Name, actorLookupRequestBuffer);

            bool lookedup = waitingResponse.Wait(TimeSpan.FromSeconds(15));
            _centerChannel.DataReceived -= onDataReceived;
            waitingResponse.Dispose();

            if (lookedup && lookupResponseEvent != null)
            {
                ActorFrameHeader actorLookupResponseFrameHeader = null;
                bool isHeaderDecoded = _frameBuilder.TryDecodeFrameHeader(
                    lookupResponseEvent.Data, lookupResponseEvent.DataOffset, lookupResponseEvent.DataLength,
                    out actorLookupResponseFrameHeader);
                if (isHeaderDecoded && actorLookupResponseFrameHeader.OpCode == OpCode.Here)
                {
                    byte[] payload;
                    int payloadOffset;
                    int payloadCount;
                    _frameBuilder.DecodePayload(
                        lookupResponseEvent.Data, lookupResponseEvent.DataOffset, actorLookupResponseFrameHeader,
                        out payload, out payloadOffset, out payloadCount);
                    var actorLookupResponseData = _frameBuilder.ControlFrameDataDecoder.DecodeFrameData<ActorDescriptionCollection>(
                        payload, payloadOffset, payloadCount);

                    var actors = actorLookupResponseData != null ? actorLookupResponseData.Items : null;
                    if (actors != null)
                    {
                        _log.InfoFormat("Lookup actors [{0}].", actors.Count);
                        var actor = matchActorFunc(actors);
                        if (actor != null)
                        {
                            IPAddress actorAddress = ResolveIPAddress(actor.Address);
                            int actorPort = int.Parse(actor.Port);
                            var actorEndPoint = new IPEndPoint(actorAddress, actorPort);
                            return actorEndPoint;
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
                            string.Format("Cannot resolve host [{0}] by DNS.", host));
                    }
                }
            }

            return remoteIPAddress;
        }
    }
}
