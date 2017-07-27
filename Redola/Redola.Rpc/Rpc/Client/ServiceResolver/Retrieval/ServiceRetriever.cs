using System;
using System.Collections.Generic;
using Redola.ActorModel;

namespace Redola.Rpc
{
    public class ServiceRetriever : IServiceRetriever
    {
        public ServiceRetriever()
        {

        }

        public IEnumerable<ActorIdentity> Retrieve(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
