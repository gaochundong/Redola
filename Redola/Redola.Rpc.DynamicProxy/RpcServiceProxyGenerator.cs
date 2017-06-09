using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Redola.Rpc.DynamicProxy
{
    public interface IService
    {
        int Sum(int a, int b);
    }

    public abstract class AbstractService
    {
        public abstract int Add(int a, int b);
        protected abstract int Subtract(int a, int b);
    }

    public class SetReturnValueInterceptor : IInterceptor
    {
        private object value;

        public SetReturnValueInterceptor(object value)
        {
            this.value = value;
        }


        public void Intercept(IInvocation invocation)
        {
            //if (invocation.TargetType != typeof(Foo))
            //{
            //    invocation.Proceed();
            //    return;
            //}
            if (invocation.Method.Name != "Bar")
            {
                invocation.Proceed();
                return;
            }
            if (invocation.Method.GetParameters().Length != 3)
            {
                invocation.Proceed();
                return;
            }
            //DoSomeActualWork(invocation);
        }
    }

    public class RpcServiceProxyGenerator
    {
        public void CreateProxy<T>()
        {
            ProxyGenerator generator = new ProxyGenerator();
            var service = generator.CreateInterfaceProxyWithoutTarget(
                typeof(AbstractService),
                new Type[] { typeof(IService) },
                new SetReturnValueInterceptor(9));

            var result1 = ((AbstractService)service).Add(1, 2);
            var result2 = ((IService)service).Sum(3, 4);
        }
    }
}
