using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redola.Rpc
{
    public class MethodRouteBuilder
    {
        private MethodLocatorExtractor _extractor;

        public MethodRouteBuilder(MethodLocatorExtractor extractor)
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

                var methods = serviceType.GetMethods(BindingFlags.Public | BindingFlags.InvokeMethod);
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
