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

		private string url()
		{
			var brokerUri = new Uri("x://");
			if (_url != null && _url.Url != null)
				brokerUri = new Uri(_url.Url);
			return new Uri(brokerUri, "/MessageBroker/NotifyClients").ToString();
		}

		public void Send(Notification notification)
		{
			var content = _seralizer.SerializeObject(notification);
			PostAsync(_httpClient, url(), new StringContent(content, Encoding.UTF8, "application/json"));
		}

		public void SendMultiple(IEnumerable<Notification> notifications)
		{
			notifications.ForEach(Send);
		}
	}
}