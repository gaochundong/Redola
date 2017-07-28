using System;
using System.Reflection;

namespace Redola.Rpc
{
    public class MethodRoute
    {
        public MethodRoute(string locator, object instance, MethodInfo method)
        {
            if (string.IsNullOrWhiteSpace(locator))
                throw new ArgumentNullException("locator");
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (method == null)
                throw new ArgumentNullException("method");

            this.Locator = locator;
            this.Instance = instance;
            this.Method = method;
        }

        public string Locator { get; private set; }
        public object Instance { get; private set; }
        public MethodInfo Method { get; private set; }

        public void Invoke(object[] methodArguments)
        {
            this.Method.Invoke(this.Instance, methodArguments);
        }

        public object InvokeReturn(object[] methodArguments)
        {
            return this.Method.Invoke(this.Instance, methodArguments);
        }
    }
}
