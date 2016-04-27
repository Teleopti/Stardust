using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Stardust.Manager.Interfaces;

namespace ManagerTest.Fakes
{
	public class FakeHttpSender : IHttpSender
	{
		public FakeHttpSender()
		{
			BusyNodesUrl = new List<string>();
			CallToWorkerNodes = new List<string>();
		}

		public List<string> BusyNodesUrl { get; set; }

		public List<string> CallToWorkerNodes { get; set; }

		public Task<HttpResponseMessage> PostAsync(Uri url,
		                                           object data)
		{
			return ReturnOkOrConflict(url);
		}

		public Task<HttpResponseMessage> PutAsync(Uri url, object data)
		{
			CallToWorkerNodes.Add(url.ToString());

			var task = new Task<HttpResponseMessage>(() => new HttpResponseMessage(HttpStatusCode.OK));

			task.Start();
			task.Wait();

			return task;
		}

		public Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			CallToWorkerNodes.Add(url.ToString());

			var task = new Task<HttpResponseMessage>(() => new HttpResponseMessage(HttpStatusCode.OK));

			task.Start();
			task.Wait();

			return task;
		}

		public Task<HttpResponseMessage> GetAsync(Uri url)
		{
			return ReturnOkOrConflict(url);
		}

		public Task<bool> TryGetAsync(Uri url)
		{
			var task = new Task<bool>(() => true);
			task.Start();
			task.Wait();

			return task;
		}

		public Task<HttpResponseMessage> TryPostAsync(Uri url, object data)
		{
			CallToWorkerNodes.Add(url.ToString());

			var task = new Task<HttpResponseMessage>(() => new HttpResponseMessage(HttpStatusCode.OK));

			task.Start();
			task.Wait();

			return task;
		}

		private Task<HttpResponseMessage> ReturnOkOrConflict(Uri url)
		{
			CallToWorkerNodes.Add(url.ToString());
			HttpStatusCode statusCode = HttpStatusCode.OK;
			if (BusyNodesUrl.Any(url.ToString().Contains))
			{
				statusCode = HttpStatusCode.Conflict;
			}

			var task = new Task<HttpResponseMessage>(() => new HttpResponseMessage(statusCode));

			task.Start();
			task.Wait();

			return task;
		}
	}
}