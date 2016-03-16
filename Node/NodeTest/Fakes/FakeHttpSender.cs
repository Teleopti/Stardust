using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes
{
	public class FakeHttpSender : IHttpSender
	{
		public string CalledUrl { get; private set; }

		public string SentJson { get; private set; }

#pragma warning disable 1998
		public async Task<HttpResponseMessage> PostAsync(Uri url, object data, CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			SentJson = data.ToString();
			CalledUrl = url.ToString();

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

#pragma warning disable 1998
		public async Task<HttpResponseMessage> PostAsync(Uri url, object data)
#pragma warning restore 1998
		{
			SentJson = data.ToString();
			CalledUrl = url.ToString();

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

#pragma warning disable 1998
		public async  Task<HttpResponseMessage> DeleteAsync(Uri url)
#pragma warning restore 1998
		{
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

#pragma warning disable 1998
		public async Task<HttpResponseMessage> DeleteAsync(Uri url, CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

#pragma warning disable 1998
		public async Task<HttpResponseMessage> GetAsync(Uri url)
#pragma warning restore 1998
		{
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

#pragma warning disable 1998
		public async Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken cancellationToken)
#pragma warning restore 1998
		{
			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		public Task<bool> TryGetAsync(Uri url)
		{
			var task = new Task<bool>(() => true);
			task.Start();
			task.Wait();

			return task;

		}

		public Task<bool> TryGetAsync(Uri url, CancellationToken cancellationToken)
		{
			var task = new Task<bool>(() => true,cancellationToken);
			task.Start();
			task.Wait(cancellationToken);

			return task;

		}
	}
}