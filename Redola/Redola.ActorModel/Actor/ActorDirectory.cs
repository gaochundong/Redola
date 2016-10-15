using System;
using System.Linq;
using System.Net;
using System.Threading;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class ActorDirectory
    {
        private ILog _log = Logger.Get<ActorDirectory>();
        private ActorDescription _centerActor;
        private IActorChannel _centerChannel;
        private IActorMessageEncoder _encoder;
        private IActorMessageDecoder _decoder;

        public ActorDirectory(
            ActorDescription centerActor, IActorChannel centerChannel,
            IActorMessageEncoder encoder, IActorMessageDecoder decoder)
        {
            if (centerActor == null)
                throw new ArgumentNullException("centerActor");
            if (centerChannel == null)
                throw new ArgumentNullException("centerChannel");
            if (encoder == null)
                throw new ArgumentNullException("encoder");
            if (decoder == null)
                throw new ArgumentNullException("decoder");

            _centerActor = centerActor;
            _centerChannel = centerChannel;
            _encoder = encoder;
            _decoder = decoder;
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
            var actorLookupRequest = new ActorLookupRequest();
            var actorLookupRequestBuffer = _encoder.Encode(actorLookupRequest);

            ManualResetEventSlim waitingResponse = new ManualResetEventSlim(false);
            ActorDataReceivedEventArgs responseEvent = null;
            EventHandler<ActorDataReceivedEventArgs> onDataReceived =
                (s, e) =>
                {
                    responseEvent = e;
                    waitingResponse.Set();
                };

            _centerChannel.DataReceived += onDataReceived;
            _centerChannel.BeginSend(_centerActor.Type, _centerActor.Name, actorLookupRequestBuffer);

            bool lookedup = waitingResponse.Wait(TimeSpan.FromSeconds(15));
            _centerChannel.DataReceived -= onDataReceived;
            waitingResponse.Dispose();

            if (lookedup && responseEvent != null)
            {
                var actorLookupResponse = _decoder.Decode<ActorLookupResponse>(
                    responseEvent.Data, responseEvent.DataOffset, responseEvent.DataLength);
                var actors = actorLookupResponse.Actors;
                if (actors != null)
                {
                    _log.InfoFormat("Lookup actors [{0}].", actors.Count);
                    var actor = actors.FirstOrDefault(a => a.Type == actorType && a.Name == actorName);
                    if (actor != null)
                    {
                        IPAddress actorAddress = ResolveIPAddress(actor.Address);
                        int actorPort = int.Parse(actor.Port);
                        var actorEndPoint = new IPEndPoint(actorAddress, actorPort);
                        return actorEndPoint;
                    }
                }
            }

            throw new ActorNotFoundException(string.Format(
                "Cannot lookup remote actor, Type[{0}], Name[{1}].", actorType, actorName));
        }

        public IPEndPoint LookupRemoteActorEndPoint(string actorType)
        {
            var actorLookupRequest = new ActorLookupRequest();
            var actorLookupRequestBuffer = _encoder.Encode(actorLookupRequest);

            ManualResetEventSlim waitingResponse = new ManualResetEventSlim(false);
            ActorDataReceivedEventArgs responseEvent = null;
            EventHandler<ActorDataReceivedEventArgs> onDataReceived =
                (s, e) =>
                {
                    responseEvent = e;
                    waitingResponse.Set();
                };

            _centerChannel.DataReceived += onDataReceived;
            _centerChannel.BeginSend(_centerActor.Type, _centerActor.Name, actorLookupRequestBuffer);

            bool lookedup = waitingResponse.Wait(TimeSpan.FromSeconds(15));
            _centerChannel.DataReceived -= onDataReceived;
            waitingResponse.Dispose();

            if (lookedup && responseEvent != null)
            {
                var actorLookupResponse = _decoder.Decode<ActorLookupResponse>(
                    responseEvent.Data, responseEvent.DataOffset, responseEvent.DataLength);
                var actors = actorLookupResponse.Actors;
                if (actors != null)
                {
                    _log.InfoFormat("Lookup actors [{0}], [{1}].", actors.Count, string.Join(",", actors.Select(a => a.Type).Distinct().ToArray()));
                    var actor = actors.Where(a => a.Type == actorType).OrderBy(t => Guid.NewGuid()).FirstOrDefault();
                    if (actor != null)
                    {
                        IPAddress actorAddress = ResolveIPAddress(actor.Address);
                        int actorPort = int.Parse(actor.Port);
                        var actorEndPoint = new IPEndPoint(actorAddress, actorPort);
                        return actorEndPoint;
                    }
                }
            }

            throw new ActorNotFoundException(string.Format(
                "Cannot lookup remote actor, Type[{0}].", actorType));
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
