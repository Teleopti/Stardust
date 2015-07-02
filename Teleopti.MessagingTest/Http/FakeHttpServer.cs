using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.MessagingTest.Http
{
	public class FakeHttpServer : IHttpServer
	{
		private readonly IJsonSerializer _serializer;
		private readonly IList<Message> _messages = new List<Message>();
		private bool _shouldFail;
		private HttpStatusCode _statusCode;

		public FakeHttpServer(IJsonSerializer serializer)
		{
			_serializer = serializer;
		}

		public int CallsToCreateMailbox { get; private set; }

		public void Has(Message message)
		{
			_messages.Add(message);
		}

		public Task<HttpResponseMessage> PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
			if (uri.Contains("AddMailbox")) 
				CallsToCreateMailbox++;
			if (_shouldFail)
				return Task.FromResult(new HttpResponseMessage {StatusCode = _statusCode});
			return Task.FromResult(new HttpResponseMessage());
		}

		public Task<HttpResponseMessage> GetAsync(HttpClient client, string uri)
		{
			if (_shouldFail)
				return Task.FromResult(new HttpResponseMessage { StatusCode = _statusCode });

			var result = _serializer.SerializeObject(_messages);
			_messages.Clear();
			return Task.FromResult(new HttpResponseMessage {Content = new StringContent(result)});
		}

		public void Fails(HttpStatusCode statusCode)
		{
			_shouldFail = true;
			_statusCode = statusCode;
		}

		public void Succeds()
		{
			_shouldFail = false;
			_statusCode = HttpStatusCode.OK;
		}
	}
}