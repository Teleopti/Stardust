namespace Stardust.Node.Interfaces
{
    public interface IPostHttpRequest
    {
        T Send<T>(string url, string json, string userAgent = null);
    }
}