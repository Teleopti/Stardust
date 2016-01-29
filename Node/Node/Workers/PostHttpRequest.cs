using System.Net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
    public class PostHttpRequest : IPostHttpRequest
    {
        public T Send<T>(string url,
                         string json,
                         string userAgent = null)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.UserAgent = userAgent;
            request.Method = "POST";
            return request.PostRequest<T>(json);
        }
    }
}