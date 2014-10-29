using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Teleopti.Analytics.Stats.TestApplication
{
	public class WebApiClient
	{
		public async Task<bool> PostQueueDataAsync(string baseUrl, QueueStatsModel queueStatsModel)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(baseUrl);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var postBody = JsonConvert.SerializeObject(queueStatsModel);
				var response = await client.PostAsync("api/mart/QueueStats", new StringContent(postBody, Encoding.UTF8, "application/json"));
				return response.IsSuccessStatusCode;
			}
		}
	}
}