using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Polly;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class PostHttpRequest : IPostHttpRequest
	{
		private readonly HttpClient client = new HttpClient();

		public T Send<T>(string url, string json, string userAgent = null)
		{
			var request = new HttpRequestMessage(HttpMethod.Post, url);
			if (userAgent != null)
			{
				request.Headers.UserAgent.Clear();
				request.Headers.Add("User-Agent", userAgent);
			}

			request.Content = new StringContent(json, Encoding.UTF8, "application/json");

			var returnValue = Policy.Handle<HttpRequestException>()
				.WaitAndRetry(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
				.Execute(() =>
				{
					var result = client.SendAsync(request);
					return JsonConvert.DeserializeObject<T>(result.Result.Content.ReadAsStringAsync().Result);
				});

			return returnValue;
		}

		public T SendSecured<T>(string url, string json, TenantCredentials tenantCredentials)
		{
			const string PersonIdHeader = "personid";
			const string TenantPasswordHeader = "tenantpassword";

			var request = new HttpRequestMessage(HttpMethod.Post, url);
			
			request.Content = new StringContent(json, Encoding.UTF8, "application/json");

			request.Headers.Add(PersonIdHeader, tenantCredentials.PersonId.ToString());
			request.Headers.Add(TenantPasswordHeader, tenantCredentials.TenantPassword);

			var returnValue = Policy.Handle<HttpRequestException>()
				.WaitAndRetry(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
				.Execute(() =>
				{
					var result = client.SendAsync(request);
					return JsonConvert.DeserializeObject<T>(result.Result.Content.ReadAsStringAsync().Result);
				});

			return returnValue;
		}
	}
}