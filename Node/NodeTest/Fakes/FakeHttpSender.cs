using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
			SentJson = JsonConvert.SerializeObject(data);
			CalledUrl = url.ToString();

			var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				RequestMessage = new HttpRequestMessage()
			};

			return responseMessage;
		}
	}
}