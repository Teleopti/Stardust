using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Teleopti.Analytics.Stats.TestApplication
{
	public class WebApiClient
	{
		private readonly HttpClient _client;
		private readonly int _latency;

		public WebApiClient(HttpClient client, string nhibDataSourcename, int queueDataSourceId)
		{
			_latency = int.Parse(ConfigurationManager.AppSettings["Latency"]);
			_client = client;
			_client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebApiBaseUrl"]);
			_client.DefaultRequestHeaders.Accept.Clear();
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			_client.DefaultRequestHeaders.Add("Authorization", authorizeHeader(nhibDataSourcename));
			_client.DefaultRequestHeaders.Add("database", nhibDataSourcename);
			_client.DefaultRequestHeaders.Add("sourceId", queueDataSourceId.ToString(CultureInfo.InvariantCulture));
			_client.DefaultRequestHeaders.Add("dbLatency", _latency.ToString(CultureInfo.InvariantCulture));
			
		}

		private static string authorizeHeader(string nhibDataSourcename)
		{
			var authKey = ConfigurationManager.AppSettings["AuthenticationKey"];
			var authText = string.Format("{0}:{1}", nhibDataSourcename, authKey);
			return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(authText));
		}

		public async Task<bool> PostQueueDataAsync(IEnumerable<QueueStatsModel> queueStatsModels, bool useLatency)
		{
			var postBody = JsonConvert.SerializeObject(queueStatsModels);
			if (_latency > 0)
				Thread.Sleep(_latency);
			var response = await _client.PostAsync("api/mart/QueueStats/PostIntervals", new StringContent(postBody, Encoding.UTF8, "application/json"));

			return response.IsSuccessStatusCode;
		}
	}
}