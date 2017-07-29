using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public interface IServiceRetriever
    {
        IEnumerable<ServiceActor> Retrieve(Type serviceType);
    }
}
