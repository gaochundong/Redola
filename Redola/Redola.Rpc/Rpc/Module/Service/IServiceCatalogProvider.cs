using System;
using System.Collections.Generic;

namespace Redola.Rpc
{
    public interface IServiceCatalogProvider
    {
        void RegisterService<T>(T service);
        void RegisterService(Type declaringType, object service);
        IEnumerable<ServiceEntry> GetServices();
    }
}
