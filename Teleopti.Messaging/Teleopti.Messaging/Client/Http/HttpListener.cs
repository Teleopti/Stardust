using System;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{

	public class HttpListener : IMessageListener, IDisposable
	{
		private readonly MailboxPoller _mailboxPoller;
		private readonly IConfigReader _config;

		public HttpListener(
			EventHandlers eventHandlers, 
			IJsonDeserializer jsonDeserializer, 
			ITime time, 
			IConfigReader config, 
			HttpClientM client)
		{
			_config = config;
			_mailboxPoller = new MailboxPoller(eventHandlers, time, client, jsonDeserializer);
		}
		
		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_mailboxPoller.StartPollingFor(
				subscription, 
				eventMessageHandler,
				TimeSpan.FromSeconds(_config.ReadValue("MessageBrokerMailboxPollingIntervalInSeconds", 60)));
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