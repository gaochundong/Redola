using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Consul;
using Logrila.Logging;
using Redola.ActorModel;

namespace Redola.Rpc.ServiceDiscovery.ConsulIntegration
{
    public class ConsulServiceRegistry
    {
        private ILog _log = Logger.Get<ConsulServiceRegistry>();
        private ConsulClient _consul;
        private const string _protocol = @"redola";

        public ConsulServiceRegistry(ConsulClient consul)
        {
            if (consul == null)
                throw new ArgumentNullException("consul");
            _consul = consul;
        }

        public void RegisterService(ActorIdentity actor, string serviceType)
        {
            RegisterService(actor, serviceType, null);
        }

        public void RegisterService(ActorIdentity actor, string serviceType, IEnumerable<string> tags)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (string.IsNullOrWhiteSpace(serviceType))
                throw new ArgumentNullException("serviceType");

            var registration = new AgentServiceRegistration()
            {
                ID = string.Format("{0}/{1}/{2}/{3}", _protocol, actor.Type, actor.Name, serviceType),
                Name = serviceType,
                Tags = tags == null ? null : tags.ToArray(),
                Address = actor.Address,
                Port = int.Parse(actor.Port),
                EnableTagOverride = true,
            };

            var result = _consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot register the actor [{0}] and service [{1}] and tags [{2}] with result [{3}] and cost [{4}] milliseconds.",
                    actor, serviceType, tags == null ? string.Empty : string.Join(",", tags), result.StatusCode, result.RequestTime.TotalMilliseconds));
            }

            _log.DebugFormat("RegisterService, register the actor [{0}] and service [{1}] and tags [{2}] with result [{3}] and cost [{4}] milliseconds.",
                actor, serviceType, tags == null ? string.Empty : string.Join(",", tags), result.StatusCode, result.RequestTime.TotalMilliseconds);
        }

        public void DeregisterService(string actorType, string actorName, string serviceType)
        {
            if (string.IsNullOrWhiteSpace(actorType))
                throw new ArgumentNullException("actorType");
            if (string.IsNullOrWhiteSpace(actorName))
                throw new ArgumentNullException("actorName");
            if (string.IsNullOrWhiteSpace(serviceType))
                throw new ArgumentNullException("serviceType");

            var serviceID = string.Format("{0}/{1}/{2}/{3}", _protocol, actorType, actorName, serviceType);

            var result = _consul.Agent.ServiceDeregister(serviceID).GetAwaiter().GetResult();

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot deregister the service [{0}] with result [{1}] and cost [{2}] milliseconds.",
                    serviceID, result.StatusCode, result.RequestTime.TotalMilliseconds));
            }

            _log.DebugFormat("DeregisterActor, deregister the service [{0}] with result [{1}] and cost [{2}] milliseconds.",
                serviceID, result.StatusCode, result.RequestTime.TotalMilliseconds);
        }

        public IEnumerable<ConsulServiceRegistryEntry> GetServices(string serviceType)
        {
            if (string.IsNullOrWhiteSpace(serviceType))
                throw new ArgumentNullException("serviceType");

            var result = _consul.Catalog.Service(serviceType).GetAwaiter().GetResult();

            if (result.StatusCode != HttpStatusCode.OK || result.Response == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot get service type [{0}] with result [{1}] and cost [{2}] milliseconds.",
                    serviceType, result.StatusCode, result.RequestTime.TotalMilliseconds));
            }

            _log.DebugFormat("GetServices, get service type [{0}] with count [{1}] and result [{2}] and cost [{3}] milliseconds.",
                serviceType, result.Response.Count(), result.StatusCode, result.RequestTime.TotalMilliseconds);

            return result.Response.Select(r =>
                {
                    var splitter = r.ServiceID.Split('/');
                    return new ConsulServiceRegistryEntry()
                    {
                        ServiceType = serviceType,
                        ServiceActor = new ActorIdentity(splitter[1], splitter[2])
                        {
                            Address = r.ServiceAddress,
                            Port = r.ServicePort.ToString(),
                        },
                    };
                });
        }
    }
}
