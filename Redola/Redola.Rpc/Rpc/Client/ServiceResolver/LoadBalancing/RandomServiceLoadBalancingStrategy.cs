using System;
using System.Collections.Generic;
using System.Linq;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class RandomServiceLoadBalancingStrategy : IServiceLoadBalancingStrategy
    {
        public ServiceActor Select(IEnumerable<ServiceActor> services)
        {
            return services.OrderBy(t => Guid.NewGuid()).FirstOrDefault();
        }
    }
}
