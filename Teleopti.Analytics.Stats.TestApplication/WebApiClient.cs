using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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

		public WebApiClient(HttpClient client, string nhibDataSourcename, int queueDataSourceId)
		{
			_client = client;
			_client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebApiBaseUrl"]);
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client.DefaultRequestHeaders.Add("database", nhibDataSourcename);
			_client.DefaultRequestHeaders.Add("sourceId", queueDataSourceId.ToString(CultureInfo.InvariantCulture));
		}

		public async Task<bool> PostQueueDataAsync(IEnumerable<QueueStatsModel> queueStatsModels)
		{
			var postBody = JsonConvert.SerializeObject(queueStatsModels);
			var response = await _client.PostAsync("api/mart/QueueStats", new StringContent(postBody, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode;
		}
	}
}