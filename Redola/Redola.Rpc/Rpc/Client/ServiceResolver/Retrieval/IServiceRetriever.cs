using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public interface IServiceRetriever
    {
        IEnumerable<ActorIdentity> Retrieve(Type serviceType);
    }
}
