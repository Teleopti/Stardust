using System.Collections.Generic;
using System.Net.Http;
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

		public void PostAsync(HttpClient client, string uri, HttpContent httpContent)
		{
		}

		public string Get(HttpClient client, string uri)
		{
			var result = _serializer.SerializeObject(_messages);
			_messages.Clear();
			return result;
		}
	}
}