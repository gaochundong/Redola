using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceDiscovery
    {
        IEnumerable<ActorIdentity> Discover(Type serviceType);
    }
}
