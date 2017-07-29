using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public interface IServiceDiscovery
    {
        IEnumerable<ServiceActor> Discover(Type serviceType);
    }
}
