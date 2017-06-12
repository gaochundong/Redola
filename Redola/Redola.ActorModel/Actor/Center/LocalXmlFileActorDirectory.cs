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
            _log.DebugFormat("Lookup actors, ActorType[{0}], Count[{1}].", actorType, _configuration.ActorDirectory.Count());
            var matchedActors = matchActorFunc(_configuration.ActorDirectory);
            if (matchedActors != null && matchedActors.Any())
            {
                _log.DebugFormat("Resolve actors, ActorType[{0}], Count[{1}].", actorType, matchedActors.Count());
                return matchedActors;
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
