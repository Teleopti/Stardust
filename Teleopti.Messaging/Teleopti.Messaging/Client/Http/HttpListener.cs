using System;
using System.Globalization;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpListener : IMessageListener, IDisposable
	{
		private readonly MailboxPoller _mailboxPoller;
		private readonly IConfigurationWrapper _configurationWrapper;

		public HttpListener(
			EventHandlers eventHandlers, 
			IHttpServer httpServer, 
			IMessageBrokerUrl url,
			IJsonSerializer jsonSerializer,
			IJsonDeserializer jsonDeserializer,
			ITime time, 
			IConfigurationWrapper configurationWrapper)
		{
			_configurationWrapper = configurationWrapper;
			var client = new HttpRequests(url, jsonSerializer)
			{
				PostAsync = (c, uri, content) => httpServer.PostAsync(c, uri, content),
				GetAsync = (c, uri) => httpServer.GetAsync(c, uri)
			};
			_mailboxPoller = new MailboxPoller(eventHandlers, time, client, jsonDeserializer);
		}
		
		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_mailboxPoller.StartPollingFor(subscription, eventMessageHandler, getPollingIntervalFromConfig());
		}

		private TimeSpan getPollingIntervalFromConfig()
		{
			string rawInteraval;
			var pollingInterval = _configurationWrapper.AppSettings.TryGetValue(
				"MessageBrokerMailboxPollingIntervalInSeconds", out rawInteraval)
				? Convert.ToDouble(rawInteraval, CultureInfo.InvariantCulture)
				: 60;
			return TimeSpan.FromSeconds(pollingInterval);
		}

		public bool IsAlive()
		{
			return _mailboxPoller.AreAllPollersAlive();
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_mailboxPoller.StopPollingOf(eventMessageHandler);
		}

		public void Dispose()
		{
			_mailboxPoller.Dispose();
		}
	}
}