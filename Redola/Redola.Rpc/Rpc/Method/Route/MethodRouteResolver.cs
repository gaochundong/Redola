using System;

namespace Redola.Rpc
{
    public class MethodRouteResolver
    {
        private MethodRouteCache _routeCache;

        public MethodRouteResolver(MethodRouteCache routeCache)
        {
            if (routeCache == null)
                throw new ArgumentNullException("routeCache");

            _routeCache = routeCache;
        }

        public MethodRoute Resolve(string methodLocator)
        {
            if (string.IsNullOrWhiteSpace(methodLocator))
                throw new ArgumentNullException("methodLocator");

            MethodRoute route = null;
            if (!_routeCache.TryGetValue(methodLocator, out route) || route == null)
                throw new InvalidOperationException(string.Format(
                    "Cannot resolve method route [{0}].", methodLocator));

            return route;
        }
    }
}
