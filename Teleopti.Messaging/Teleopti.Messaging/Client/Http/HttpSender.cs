using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpSender : IMessageSender
	{
		private readonly IMessageBrokerUrl _url;
		private readonly IJsonSerializer _seralizer;

		public Action<HttpClient, string, HttpContent> PostAsync =
			(client, uri, httpContent) => client.PostAsync(uri, httpContent);

		private readonly HttpClient _httpClient = new HttpClient();

		public HttpSender(IMessageBrokerUrl url, IJsonSerializer seralizer)
		{
			_url = url;
			_seralizer = seralizer ?? new ToStringSerializer();
		}

		private string url(string call)
		{
			if (_url == null)
				return null;
			if (string.IsNullOrEmpty(_url.Url))
				return null;
			return new Uri(new Uri(_url.Url), call).ToString();
		}

		public void Send(Notification notification)
		{
			var content = _seralizer.SerializeObject(notification);
			PostAsync(_httpClient, url("/MessageBroker/NotifyClients"), new StringContent(content, Encoding.UTF8, "application/json"));
		}

		public void SendMultiple(IEnumerable<Notification> notifications)
		{
			var content = _seralizer.SerializeObject(notifications);
			PostAsync(_httpClient, url("/MessageBroker/NotifyClientsMultiple"), new StringContent(content, Encoding.UTF8, "application/json"));
		}
	}
}