using System.Reflection;

namespace Redola.Rpc
{
    public interface IMethodLocatorExtractor
    {
        string Extract(MethodInfo method);
    }
}
