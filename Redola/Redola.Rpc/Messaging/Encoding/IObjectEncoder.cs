
namespace Redola.Rpc
{
    public interface IObjectEncoder
    {
        byte[] Encode(object obj);
        byte[] Encode<T>(T obj);
    }
}
