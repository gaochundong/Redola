using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceRetriever
    {
        IEnumerable<ServiceActor> Retrieve(Type serviceType);
    }
}
