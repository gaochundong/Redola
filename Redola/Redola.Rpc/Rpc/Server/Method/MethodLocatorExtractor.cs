using System;
using System.Linq;
using System.Reflection;

namespace Redola.Rpc
{
    public class MethodLocatorExtractor
    {
        public MethodLocatorExtractor()
        {
        }

        public string Extract(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            var serviceType = method.DeclaringType;

            var methodLocator = string.Format("{0}/{1}", serviceType.FullName, method.Name);

            var parameters = method.GetParameters();
            if (parameters != null && parameters.Length > 0)
            {
                methodLocator = string.Format("{0}/{1}_{2}", serviceType.FullName, method.Name,
                    string.Join("_", parameters.Select(p => p.ParameterType.Name)));
            }

            return methodLocator;
        }
    }
}
