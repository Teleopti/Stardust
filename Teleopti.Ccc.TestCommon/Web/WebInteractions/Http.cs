using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Teleopti.Ccc.TestCommon.Web.WebInteractions
{
	public class Http : IDisposable
	{
		private readonly HttpClientHandler _handler;
		private readonly HttpClient _client;

		public Http()
		{
			_handler = new HttpClientHandler();
			_handler.AllowAutoRedirect = false;
			_client = new HttpClient(_handler);
		}

		private static Uri uri(string url)
		{
			return new Uri(TestSiteConfigurationSetup.URL, url);
		}

		public void PostJson(string url, object data)
		{
			var json = JsonConvert.SerializeObject(data);
			var request = new HttpRequestMessage
			{
				RequestUri = uri(url),
				Method = HttpMethod.Post,
				Content = new StringContent(json, Encoding.UTF8, "application/json")
			};
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = _client.SendAsync(request).GetAwaiter().GetResult();

			if (new[] {HttpStatusCode.OK, HttpStatusCode.Created}
				.Contains(response.StatusCode))
				return;

			var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			throw new Exception($"Posting json returned http code {response.StatusCode}, Sent: {json}, Response: {responseContent}");
		}

		public void GetJson(string url)
		{
			var request = new HttpRequestMessage
			{
				RequestUri = uri(url),
				Method = HttpMethod.Get,
			};
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = _client.SendAsync(request).GetAwaiter().GetResult();

			if (new[] {HttpStatusCode.OK}
				.Contains(response.StatusCode))
				return;

			var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
			throw new Exception($"Get json returned http code {response.StatusCode}, Response: {responseContent}");
		}

		// will "sometimes always" return 200 for html if the server responds with friendly custom error page
		public void Get(string url)
		{
			var post = _client.GetAsync(uri(url));
			var response = post.GetAwaiter().GetResult();
			if (response.StatusCode != HttpStatusCode.OK)
			{
				var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				throw new Exception("Get returned http code " + response.StatusCode + ", Content: " + responseContent);
			}
		}

		public void GetAsync(string url)
		{
			_client.GetAsync(uri(url));
		}

		public void Dispose()
		{
			_client?.Dispose();
			_handler?.Dispose();
		}
	}
}