using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Polly;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class GetHttpRequest : IGetHttpRequest
	{
		private readonly HttpClient client = new HttpClient();

		public T Get<T>(string url, NameValueCollection arguments)
		{
			url = appendQueryStringArguments(url, arguments);
			
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			
			var returnValue = Policy.Handle<HttpRequestException>()
				.Or<AggregateException>(ex => ex.InnerExceptions.Any(e => e is HttpRequestException))
				.WaitAndRetry(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
				.Execute(() =>
				{
					var result = client.SendAsync(request);
					return JsonConvert.DeserializeObject<T>(result.Result.Content.ReadAsStringAsync().Result);
				});

			return returnValue;
		}

		private static string appendQueryStringArguments(string url, NameValueCollection arguments)
		{
			var builder = new StringBuilder(url);
			builder.Append(url.Contains("?") ? "&" : "?");
			foreach (var argument in arguments.AllKeys)
			{
				builder.Append(argument);
				builder.Append("=");
				builder.Append(Uri.EscapeDataString(arguments[argument]));
				builder.Append("&");
			}
			url = builder.ToString();
			return url;
		}

		public T GetSecured<T>(string url, NameValueCollection arguments, TenantCredentials tenantCredentials)
		{
			url = appendQueryStringArguments(url, arguments);

			const string PersonIdHeader = "personid";
			const string TenantPasswordHeader = "tenantpassword";

			var request = new HttpRequestMessage(HttpMethod.Get, url);

			request.Headers.Add(PersonIdHeader, tenantCredentials.PersonId.ToString());
			request.Headers.Add(TenantPasswordHeader, tenantCredentials.TenantPassword);

			var returnValue = Policy.Handle<HttpRequestException>()
				.Or<AggregateException>(ex => ex.InnerExceptions.Any(e => e is HttpRequestException))
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