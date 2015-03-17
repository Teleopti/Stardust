using System.Net;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class PostHttpRequest : IPostHttpRequest
	{
		public T Send<T>(string url, string json, string userAgent = null)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = userAgent;

			return request.PostRequest<T>(json);
		}
	}
}