using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceLoadBalancingStrategy
    {
        ServiceActor Select(IEnumerable<ServiceActor> services);
    }
}
