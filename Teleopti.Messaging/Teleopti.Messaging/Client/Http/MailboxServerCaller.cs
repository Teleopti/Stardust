using System;
using System.Net.Http;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Messaging.Client.Http
{
	public class MailboxServerCaller
	{
		private readonly HttpClientM _client;

		public MailboxServerCaller(HttpClientM client)
		{
			_client = client;
		}

		public bool TryAddMailbox(Subscription subscription)
		{
			try
			{
				return _client.Post("MessageBroker/AddMailbox", subscription).IsSuccessStatusCode;
			}
			catch (AggregateException e)
			{
				if (e.InnerException.GetType() == typeof (HttpRequestException))
					return false;
				throw;
			}
		}

		public string TryGetMessagesFromServer(string mailboxId)
		{
			try
			{
				var content = _client.Get("MessageBroker/PopMessages/" + mailboxId).Content;
				if (content == null)
					return null;
				return content.ReadAsStringAsync().Result;
			}
			catch (AggregateException e)
			{
				if (e.InnerException.GetType() == typeof(HttpRequestException))
					return null;
				throw;
			}
		}

	}
}