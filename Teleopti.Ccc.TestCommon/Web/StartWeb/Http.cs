using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace Teleopti.Ccc.TestCommon.Web.StartWeb
{
	public static class Http
	{
		public static void PostJson(string url, object data)
		{
			var uri = new Uri(TestSiteConfigurationSetup.URL, url);

			var json = JsonConvert.SerializeObject(data);

			using (var handler = new HttpClientHandler())
			{
				handler.AllowAutoRedirect = false;
				using (var client = new HttpClient(handler))
				{
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
					var post = client.PostAsync(uri, requestContent);
					var response = post.Result;
					if (response.StatusCode != HttpStatusCode.OK)
					{
						var responseContent = response.Content.ReadAsStringAsync().Result;
						throw new Exception("Posting json returned http code " + response.StatusCode + ", Content: " + responseContent);
					}
				}
			}
		}

		public static void Get(string url)
		{
			var uri = new Uri(TestSiteConfigurationSetup.URL, url);

			using (var handler = new HttpClientHandler())
			{
				handler.AllowAutoRedirect = false;
				using (var client = new HttpClient(handler))
				{
					var post = client.GetAsync(uri);
					var response = post.Result;
					if (response.StatusCode != HttpStatusCode.OK)
					{
						var responseContent = response.Content.ReadAsStringAsync().Result;
						throw new Exception("Get returned http code " + response.StatusCode + ", Content: " + responseContent);
					}
				}
			}
		}

		public static T GetJson<T>(string url)
		{
			var uri = new Uri(TestSiteConfigurationSetup.URL, url);

			using (var handler = new HttpClientHandler())
			{
				handler.AllowAutoRedirect = false;
				using (var client = new HttpClient(handler))
				{
					var post = client.GetAsync(uri);
					var response = post.Result;
					var responseContent = response.Content.ReadAsStringAsync().Result;
					if (response.StatusCode != HttpStatusCode.OK)
					{
						throw new Exception("Get returned http code " + response.StatusCode + ", Content: " + responseContent);
					}
					return JsonConvert.DeserializeObject<T>(responseContent);
				}
			}
		}

		public static void GetAsync(string url)
		{
			var uri = new Uri(TestSiteConfigurationSetup.URL, url);

			using (var handler = new HttpClientHandler())
			{
				handler.AllowAutoRedirect = false;
				using (var client = new HttpClient(handler))
				{
					client.GetAsync(uri);
				}
			}
		}
	}
}