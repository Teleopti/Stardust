using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Stardust.Manager.Interfaces;

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
		}

		public List<string> BusyNodesUrl { get; set; }

		public List<string> CallToWorkerNodes { get; set; }

		public Task<HttpResponseMessage> PostAsync(Uri url,
		                                           object data)
		{
			if (FailPostAsync)
			{
				throw new AggregateException(new HttpRequestException("",  new WebException("The remote name could not be resolved: 'x'")));
			}
				
			return ReturnOkOrConflict(url, true);
		}

		public Task<HttpResponseMessage> PutAsync(Uri url, object data)
		{
			return ReturnOkOrConflict(url, false);
		}

		//Should Delete return more than OK?
		public Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			return ReturnOkOrConflict(url, false);
		}
		
		private Task<HttpResponseMessage> ReturnOkOrConflict(Uri url, bool canReturnConflict)
		{
			CallToWorkerNodes.Add(url.ToString());
			HttpStatusCode statusCode = HttpStatusCode.OK;
			if (BusyNodesUrl.Any(url.ToString().Contains) && canReturnConflict)
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