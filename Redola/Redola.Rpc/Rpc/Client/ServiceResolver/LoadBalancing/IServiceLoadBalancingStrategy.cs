using System.Collections.Generic;

namespace Redola.Rpc
{
    public interface IServiceLoadBalancingStrategy
    {
        ServiceActor Select(IEnumerable<ServiceActor> services);
    }
}
