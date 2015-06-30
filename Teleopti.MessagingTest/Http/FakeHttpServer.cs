using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.Http;

namespace Teleopti.MessagingTest.Http
{
	public class FakeHttpServer : IHttpServer
	{
		private readonly IJsonSerializer _serializer;
		private readonly IList<Message> _messages = new List<Message>();

		public FakeHttpServer(IJsonSerializer serializer)
		{
			_serializer = serializer;
		}

		public void Has(Message message)
		{
			_messages.Add(message);
		}

		public Task<HttpResponseMessage> PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
			return Task.FromResult(new HttpResponseMessage());
		}

		public Task<HttpResponseMessage> GetAsync(HttpClient client, string uri)
		{
			var result = _serializer.SerializeObject(_messages);
			_messages.Clear();
			return Task.FromResult(new HttpResponseMessage {Content = new StringContent(result)});
		}
	}
}