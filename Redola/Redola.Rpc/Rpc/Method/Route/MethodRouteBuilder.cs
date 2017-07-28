using System;
using System.Collections.Generic;
using System.Linq;

namespace Redola.Rpc
{
    public class MethodRouteBuilder
    {
        private IMethodLocatorExtractor _extractor;

        public MethodRouteBuilder(IMethodLocatorExtractor extractor)
        {
            if (extractor == null)
                throw new ArgumentNullException("extractor");
            _extractor = extractor;
        }

        public MethodRouteCache BuildCache(IEnumerable<ServiceEntry> entries)
        {
            var cache = new MethodRouteCache();

            foreach (var entry in entries)
            {
                var serviceType = entry.DeclaringType;
                var serviceInstance = entry.ServiceInstance;

                var methods = serviceType.GetMethods();
                foreach (var method in methods)
                {
                    var methodLocator = _extractor.Extract(method);
                    var methodInstance = serviceInstance.GetType()
                        .GetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());

                    var methodRoute = new MethodRoute(methodLocator, serviceInstance, methodInstance);
                    cache.Add(methodRoute.Locator, methodRoute);
                }
            }

            return cache;
        }
    }
}
