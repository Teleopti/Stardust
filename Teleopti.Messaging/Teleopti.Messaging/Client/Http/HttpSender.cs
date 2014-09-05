using System;
using System.Collections.Generic;
using System.Net.Http;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpSender : IMessageSender
	{
		private readonly string _url;

		public Action<HttpClient, string, HttpContent> PostAsync =
			(client, uri, httpContent) => client.PostAsync(uri, httpContent);

		private readonly HttpClient _httpClient = new HttpClient();

		public HttpSender(string url)
		{
			var brokerUri = new Uri(url ?? "x://");
			_url = new Uri(brokerUri, "/MessageBroker/NotifyClients").ToString();
		}

		public void Send(Notification notification)
		{
			PostAsync(_httpClient, _url, new StringContent(""));
		}

		public void SendMultiple(IEnumerable<Notification> notifications)
		{
			throw new NotImplementedException();
		}
	}
}