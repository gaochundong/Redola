namespace Redola.Rpc
{
    public interface IMethodArgumentEncoder
    {
        byte[] Encode(object argument);
        byte[] Encode<T>(T argument);
    }
}
