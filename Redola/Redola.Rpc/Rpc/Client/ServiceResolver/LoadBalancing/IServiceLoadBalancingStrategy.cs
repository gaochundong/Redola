using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceLoadBalancingStrategy
    {
        ActorIdentity Select(IEnumerable<ActorIdentity> services);
    }
}
