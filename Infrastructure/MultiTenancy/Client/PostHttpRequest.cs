using System.Net;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class PostHttpRequest : IPostHttpRequest
	{
		private readonly IDictionaryToPostData _iDictionaryToPostData;

		public PostHttpRequest(IDictionaryToPostData iDictionaryToPostData)
		{
			_iDictionaryToPostData = iDictionaryToPostData;
		}

		public T Send<T>(string url, string userAgent, string json)
		{
			//var post = _iDictionaryToPostData.Convert(json);

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = userAgent;

			return request.PostRequest<T>(json);
		}
	}
}