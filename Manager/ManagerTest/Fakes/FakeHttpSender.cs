using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Models;

namespace ManagerTest.Fakes
{
	public class FakeHttpSender : IHttpSender
	{
		public bool FailPostAsync;

		public FakeHttpSender()
		{
			FailPostAsync = false;
			BusyNodesUrl = new List<string>();
			CallToWorkerNodes = new List<string>();
			NodeUrlAndResult = new Dictionary<Uri, bool>();
		}

		public List<string> BusyNodesUrl { get; }

		public List<string> CallToWorkerNodes { get; }

		public Dictionary<Uri, bool> NodeUrlAndResult { get; }

		public Task<HttpResponseMessage> PostAsync(Uri url,
		                                           object data)
		{
			if (FailPostAsync)
			{
				return Task.FromException<HttpResponseMessage>(new HttpRequestException("",  new WebException("The remote name could not be resolved: 'x'")));
			}
				
			return ReturnAvailableOrBusy(url, true);
		}

		public Task<HttpResponseMessage> PutAsync(Uri url, object data)
		{
			return ReturnAvailableOrBusy(url, false);
		}

		//Should Delete return more than OK?
		public Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			return ReturnAvailableOrBusy(url, false);
		}

		public Task<HttpResponseMessage> GetAsync(Uri url)
		{
			var result = NodeUrlAndResult.FirstOrDefault(x => url.ToString().Contains(x.Key.ToString())).Value;
			var statusCode = HttpStatusCode.OK;
			if (!result)
			{
				statusCode = HttpStatusCode.NotFound;
			}
			var task = new Task<HttpResponseMessage>(() => new HttpResponseMessage(statusCode));
			task.Start();
			task.Wait();

			return task;
		}

		private Task<HttpResponseMessage> ReturnAvailableOrBusy(Uri url, bool canReturnBusy)
		{
			CallToWorkerNodes.Add(url.ToString());

			var isBusy = BusyNodesUrl.Any(url.ToString().Contains) && canReturnBusy;
			var content = JsonConvert.SerializeObject(new PrepareToStartJobResult {IsAvailable = !isBusy});
			
			var task = new Task<HttpResponseMessage>(
				() => new HttpResponseMessage(HttpStatusCode.OK) {Content = new StringContent(content)});

			task.Start();
			task.Wait();

			return task;
		}
	}
}