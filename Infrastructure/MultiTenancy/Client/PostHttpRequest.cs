using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class PostHttpRequest : IPostHttpRequest
	{
		private readonly HttpClient client = new HttpClient();

		public T Send<T>(string url, string json, string userAgent = null)
		{
			HttpRequestMessage RequestFunc()
			{
				var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = new StringContent(json, Encoding.UTF8, "application/json")};

				if (userAgent != null)
				{
					request.Headers.UserAgent.Clear();
					request.Headers.Add("User-Agent", userAgent);
				}

				return request;
			}

			var returnValue = sendWithRetry<T>(RequestFunc);

			return returnValue;
		}

		private T sendWithRetry<T>(Func<HttpRequestMessage> request)
		{
			return Policy.Handle<HttpRequestException>()
				.Or<AggregateException>(ex => ex.InnerExceptions.Any(e => e is HttpRequestException || e is TaskCanceledException))
				.WaitAndRetry(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
				.Execute(() =>
				{
					var result = client.SendAsync(request());
					return JsonConvert.DeserializeObject<T>(result.Result.Content.ReadAsStringAsync().Result);
				});
		}

		public T SendSecured<T>(string url, string json, TenantCredentials tenantCredentials)
		{
			const string PersonIdHeader = "personid";
			const string TenantPasswordHeader = "tenantpassword";

			HttpRequestMessage RequestFunc()
			{
				var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = new StringContent(json, Encoding.UTF8, "application/json")};

				request.Headers.Add(PersonIdHeader, tenantCredentials.PersonId.ToString());
				request.Headers.Add(TenantPasswordHeader, tenantCredentials.TenantPassword);
				return request;
			}

			var returnValue = sendWithRetry<T>(RequestFunc);

			return returnValue;
		}
	}
}