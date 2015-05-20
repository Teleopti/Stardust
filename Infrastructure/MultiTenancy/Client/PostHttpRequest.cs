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

		public T SendSecured<T>(string url, string json, TenantCredentials tenantCredentials)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Headers.Add("personid", tenantCredentials.PersonId.ToString());
			request.Headers.Add("tenantpassword", tenantCredentials.TenantPassword);
			return request.PostRequest<T>(json);
		}
	}
}