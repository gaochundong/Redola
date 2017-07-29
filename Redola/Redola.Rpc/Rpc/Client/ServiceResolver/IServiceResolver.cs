using System;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceResolver
    {
        ServiceActor Resolve(Type serviceType);
        ServiceActor Resolve(Type serviceType, IServiceLoadBalancingStrategy strategy);
    }
}
