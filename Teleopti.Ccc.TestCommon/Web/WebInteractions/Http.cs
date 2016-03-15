using System;
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
			var post = _client.SendAsync(request);
			var response = post.Result;
			if (response.StatusCode != HttpStatusCode.OK)
			{
				var responseContent = response.Content.ReadAsStringAsync().Result;
				throw new Exception("Posting json returned http code " + response.StatusCode + ", Content: " + responseContent);
			}
		}

		public void Get(string url)
		{
			var post = _client.GetAsync(uri(url));
			var response = post.Result;
			if (response.StatusCode != HttpStatusCode.OK)
			{
				var responseContent = response.Content.ReadAsStringAsync().Result;
				throw new Exception("Get returned http code " + response.StatusCode + ", Content: " + responseContent);
			}
		}

		public T GetJson<T>(string url)
		{
			var post = _client.GetAsync(uri(url));
			var response = post.Result;
			var responseContent = response.Content.ReadAsStringAsync().Result;
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception("Get returned http code " + response.StatusCode + ", Content: " + responseContent);
			}
			return JsonConvert.DeserializeObject<T>(responseContent);
		}

		public void GetAsync(string url)
		{
			_client.GetAsync(uri(url));
		}

		public void Dispose()
		{
			if (_client != null)
				_client.Dispose();
			if (_handler != null)
				_handler.Dispose();
		}
	}
	
}