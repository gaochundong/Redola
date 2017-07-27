using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redola.Rpc
{
    public interface IServiceProxyGenerator
    {
        T CreateServiceProxy<T>(RpcHandler handler);
    }
}
