using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Teleopti.Analytics.Stats.TestApplication
{
	public class WebApiClient
	{
		private readonly HttpClient _client;

		public WebApiClient(HttpClient client)
		{
			_client = client;
			_client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebApiBaseUrl"]);
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<bool> PostQueueDataAsync(QueueStatsModel queueStatsModel)
		{
			var postBody = JsonConvert.SerializeObject(queueStatsModel);
			var response = await _client.PostAsync("api/mart/QueueStats", new StringContent(postBody, Encoding.UTF8, "application/json"));
			
			return response.IsSuccessStatusCode;
		}
	}
}