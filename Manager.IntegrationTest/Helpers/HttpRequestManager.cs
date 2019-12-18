using System;
using System.Threading.Tasks;
using Manager.IntegrationTest.Models;
using Manager.IntegrationTest.ConsoleHost.Helpers;
using Newtonsoft.Json;

namespace Manager.IntegrationTest.Helpers
{
	public class HttpRequestManager
	{
		private HttpSender HttpSender { get; set; }
		private  ManagerUriBuilder MangerUriBuilder { get; set; }

		public HttpRequestManager()
		{
			HttpSender = new HttpSender();
			MangerUriBuilder = new ManagerUriBuilder();
		}

		public async void CancelJob(Guid jobId)
		{
			var response = HttpSender.DeleteAsync(MangerUriBuilder.GetCancelJobUri(jobId)).Result;
			await response.Content.ReadAsStringAsync();
		}

		public Guid AddJob(JobQueueItem jobQueueItem)
		{
			string jobIdString = AddJobAsync(jobQueueItem).Result;
			Guid jobId = new Guid(JsonConvert.DeserializeObject<string>(jobIdString));
			return jobId;
		}

		private async Task<string> AddJobAsync(JobQueueItem jobQueueItem)
		{
			var result = HttpSender.PostAsync(MangerUriBuilder.GetAddToJobQueueUri(), jobQueueItem).Result;
			var content = await result.Content.ReadAsStringAsync();
			return content;
		}

		public bool IsNodeWorking()
		{
			var response = HttpSender.GetAsync(new Uri("http://localhost:9050/IsWorking")).Result;
			var result = response.Content.ReadAsStringAsync().Result;
			return result.Equals("true");
		}


		public bool IsManagerUp()
		{
			try
			{
				var result = HttpSender.GetAsync(MangerUriBuilder.GetPingUri()).Result;
				if (result.IsSuccessStatusCode)
				{
					return true;
				}
				else
				{
					//manager did not respond OK on ping
					return false;
				}
				
			}
			catch
			{
				//manager did not respond
				return false;
			}
			
		} 
	}
}
