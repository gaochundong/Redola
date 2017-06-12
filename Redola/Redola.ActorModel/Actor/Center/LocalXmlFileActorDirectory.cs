using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Logrila.Logging;

namespace Redola.ActorModel
{
    public class LocalXmlFileActorDirectory : IActorDirectory
    {
        private ILog _log = Logger.Get<LocalXmlFileActorDirectory>();
        private LocalXmlFileActorConfiguration _configuration;

        public LocalXmlFileActorDirectory(LocalXmlFileActorConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");
            _configuration = configuration;
        }

        public bool Active { get; private set; }

        public void Activate(ActorIdentity localActor)
        {
            this.Active = true;
        }

        public void Close()
        {
            this.Active = false;
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

        private IEnumerable<IPEndPoint> LookupRemoteActorEndPoints(string actorType, Func<IEnumerable<ActorIdentity>, IEnumerable<ActorIdentity>> matchActorFunc)
        {
            _log.DebugFormat("Lookup actors [{0}].", _configuration.ActorDirectory.Count());
            var matchedActors = matchActorFunc(_configuration.ActorDirectory);
            if (matchedActors != null && matchedActors.Any())
            {
                var actorEndPoints = new List<IPEndPoint>();
                foreach (var item in matchedActors)
                {
                    IPAddress actorAddress = ResolveIPAddress(item.Address);
                    int actorPort = int.Parse(item.Port);
                    var actorEndPoint = new IPEndPoint(actorAddress, actorPort);
                    actorEndPoints.Add(actorEndPoint);
                }
                _log.DebugFormat("Resolve actors [{0}].", actorEndPoints.Count);
                return actorEndPoints;
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
    }
}
