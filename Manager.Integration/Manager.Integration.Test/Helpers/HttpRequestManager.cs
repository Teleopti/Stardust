using System;
using System.Threading.Tasks;
using Manager.Integration.Test.Models;
using Manager.IntegrationTest.Console.Host.Helpers;
using Newtonsoft.Json;

namespace Manager.Integration.Test.Helpers
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

		public void CancelJob(Guid jobId)
		{
			var response = HttpSender.DeleteAsync(MangerUriBuilder.GetCancelJobUri(jobId)).Result;
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
	}
}
