using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Stardust.Node.Interfaces;

namespace NodeTest.Fakes
{
	public class PostHttpRequestFake : IHttpSender
	{
		public string CalledUrl { get; private set; }

		public string SentJson { get; private set; }

		public Task<HttpResponseMessage> PostAsync(Uri url, object data, CancellationToken cancellationToken)
		{
			SentJson = data.ToString();
			CalledUrl = url.ToString();

			return null;
		}

		public Task<HttpResponseMessage> PostAsync(Uri url, object data)
		{
			SentJson = data.ToString();
			CalledUrl = url.ToString();

			return null;
		}

		public Task<HttpResponseMessage> DeleteAsync(Uri url)
		{
			throw new NotImplementedException();
		}

		public Task<HttpResponseMessage> DeleteAsync(Uri url, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<HttpResponseMessage> GetAsync(Uri url)
		{
			throw new NotImplementedException();
		}

		public Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<bool> TryGetAsync(Uri url)
		{
			throw new NotImplementedException();
		}

		public Task<bool> TryGetAsync(Uri url, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}