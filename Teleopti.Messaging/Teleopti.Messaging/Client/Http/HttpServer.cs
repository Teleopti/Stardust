using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpServer : IHttpServer
	{
		private readonly HttpClient _client;
		private readonly IJsonSerializer _serializer;

		public HttpServer(IJsonSerializer serializer) : this(null, serializer)
		{
		}

		public HttpServer(HttpClient client, IJsonSerializer serializer)
		{
			_client = client ?? new HttpClient(new HttpClientHandler {Credentials = CredentialCache.DefaultNetworkCredentials});
			_serializer = serializer ?? new ToStringSerializer();
		}

		public Task<HttpResponseMessage> Post(string uri, object data, Func<string, NameValueCollection> customHeadersFunc = null)
		{
			return innerPost(new Uri(uri), data, customHeadersFunc);
		}

		private Task<HttpResponseMessage> innerPost(Uri uri, object thing, Func<string,NameValueCollection> customHeadersFunc)
		{
			var content = _serializer.SerializeObject(thing);
			var request = new HttpRequestMessage
			{
				Content = new StringContent(content, Encoding.UTF8, "application/json"),
				Method = HttpMethod.Post,
				RequestUri = uri
			};
			if (customHeadersFunc != null)
			{
				var headers = customHeadersFunc(content);
				foreach (string header in headers.Keys)
				{
					request.Headers.TryAddWithoutValidation(header,headers[header]);
				}
			}
		
			return _client.SendAsync(request);
		}

		public void PostOrThrow(string uri, object data, Func<string,NameValueCollection> customHeadersFunc = null)
		{
			innerPost(new Uri(uri), data, customHeadersFunc).Result.EnsureSuccessStatusCode();
		}

		public async Task PostOrThrowAsync(string uri, object data, Func<string,NameValueCollection> customHeadersFunc = null)
		{
			var request = await innerPost(new Uri(uri), data, customHeadersFunc);
			request.EnsureSuccessStatusCode();
		}

		public string GetOrThrow(string uri)
		{
			return _client
				.GetAsync(uri)
				.Result
				.EnsureSuccessStatusCode()
				.Content
				.ReadAsStringAsync()
				.Result;
		}
	}
}