using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Consul;
using Logrila.Logging;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.ConsulIntegration
{
    public class ConsulActorRegistry
    {
        private ILog _log = Logger.Get<ConsulActorRegistry>();
        private ConsulClient _consul;

        public ConsulActorRegistry(ConsulClient consul)
        {
            if (consul == null)
                throw new ArgumentNullException("consul");
            _consul = consul;
        }

        public void RegisterActor(ActorIdentity actor)
        {
            RegisterActor(actor, null);
        }

        public void RegisterActor(ActorIdentity actor, IEnumerable<string> tags)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            var registration = new AgentServiceRegistration()
            {
                ID = string.Format("{0}/{1}/", actor.Type, actor.Name),
                Name = actor.Type,
                Tags = tags == null ? null : tags.ToArray(),
                Address = actor.Address,
                Port = int.Parse(actor.Port),
                EnableTagOverride = true,
            };

            var result = _consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot register the actor [{0}] and tags [{1}] with result [{2}] and cost [{3}] milliseconds.",
                    actor, string.Join(",", tags), result.StatusCode, result.RequestTime.TotalMilliseconds));
            }

            _log.DebugFormat("RegisterActor, register the actor [{0}] and tags [{1}] with result [{2}] and cost [{3}] milliseconds.",
                actor, string.Join(",", tags), result.StatusCode, result.RequestTime.TotalMilliseconds);
        }

        public void DeregisterActor(string actorType, string actorName)
        {
            if (string.IsNullOrWhiteSpace(actorType))
                throw new ArgumentNullException("actorType");
            if (string.IsNullOrWhiteSpace(actorName))
                throw new ArgumentNullException("actorName");

            var serviceID = string.Format("{0}/{1}", actorType, actorName);

            var result = _consul.Agent.ServiceDeregister(serviceID).GetAwaiter().GetResult();

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot deregister the actor [{0}] with result [{1}] and cost [{2}] milliseconds.",
                    serviceID, result.StatusCode, result.RequestTime.TotalMilliseconds));
            }

            _log.DebugFormat("DeregisterActor, deregister the actor [{0}] with result [{1}] and cost [{2}] milliseconds.",
                serviceID, result.StatusCode, result.RequestTime.TotalMilliseconds);
        }

        public IEnumerable<ConsulActorRegistryEntry> GetActors(string actorType)
        {
            if (string.IsNullOrWhiteSpace(actorType))
                throw new ArgumentNullException("actorType");

            var result = _consul.Catalog.Service(actorType).GetAwaiter().GetResult();

            if (result.StatusCode != HttpStatusCode.OK || result.Response == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot get actor type [{0}] with result [{1}] and cost [{2}] milliseconds.",
                    actorType, result.StatusCode, result.RequestTime.TotalMilliseconds));
            }

            _log.DebugFormat("GetActors, get actor type [{0}] with count [{1}] and result [{2}] and cost [{3}] milliseconds.",
                actorType, result.Response.Count(), result.StatusCode, result.RequestTime.TotalMilliseconds);

            return result.Response.Select(r =>
                new ConsulActorRegistryEntry()
                {
                    ActorKey = ActorIdentity.GetKey(r.ServiceName, r.ServiceID.Split('/')[1]),
                    ActorIdentity = new ActorIdentity(r.ServiceName, r.ServiceID.Split('/')[1])
                    {
                        Address = r.ServiceAddress,
                        Port = r.ServicePort.ToString(),
                    }
                });
        }
    }
}
