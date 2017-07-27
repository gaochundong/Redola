using System;
using System.Collections.Generic;
using System.Linq;

namespace Redola.Rpc
{
    public class ServiceCatalogProvider : IServiceCatalogProvider
    {
        private List<ServiceEntry> _entries = new List<ServiceEntry>();

        public ServiceCatalogProvider()
        {
        }

        public void RegisterService<T>(T service)
        {
            RegisterService(typeof(T), service);
        }

        public void RegisterService(Type declaringType, object service)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");
            if (service == null)
                throw new ArgumentNullException("service");

            lock (_entries)
            {
                if (_entries.Any(e => e.DeclaringType.Equals(declaringType)))
                    throw new ArgumentException(string.Format(
                        "Duplicate service entry for service type [{0}].", declaringType), "declaringType");
                _entries.Add(new ServiceEntry(declaringType, service));
            }
        }

        public IEnumerable<ServiceEntry> GetServices()
        {
            return _entries;
        }
    }
}
