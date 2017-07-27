using System;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceResolver
    {
        ActorIdentity Resolve(Type serviceType);
        ActorIdentity Resolve(Type serviceType, IServiceLoadBalancingStrategy strategy);
    }
}
