using System;
using System.Net.Http;
using System.Threading.Tasks;
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
			var result = callServer(() => _client.Post("MessageBroker/AddMailbox", subscription));
			return result != null && result.IsSuccessStatusCode;
		}

		public HttpContent TryGetMessagesFromServer(string mailboxId)
		{
			var result = callServer(() => _client.Get("MessageBroker/PopMessages/" + mailboxId));
			return result == null
				? null
				: result.Content;
		}

		private static HttpResponseMessage callServer(Func<Task<HttpResponseMessage>> call)
		{
			try
			{
				return call().Result;
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