using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public class MessageBrokerServer : IMessageBrokerServer
	{
		private readonly IActionScheduler _actionScheduler;
		private readonly ISignalR _signalR;
		private readonly IMailboxRepository _mailboxRepository;
		private readonly INow _now;
		private readonly IBeforeSubscribe _beforeSubscribe;
		public ILog Logger = LogManager.GetLogger(typeof (MessageBrokerServer));
		private readonly TimeSpan _expirationInterval;
		private DateTime _nextPurge;
	    private readonly MessageBrokerTracer _tracer = new MessageBrokerTracer();

	    public MessageBrokerServer(
			IActionScheduler actionScheduler,
			ISignalR signalR,
			IBeforeSubscribe beforeSubscribe,
			IMailboxRepository mailboxRepository,
			IConfigReader config,
			INow now)
		{
			_actionScheduler = actionScheduler;
			_signalR = signalR;
			_mailboxRepository = mailboxRepository;
			_now = now;
			_beforeSubscribe = beforeSubscribe ?? new SubscriptionPassThrough();
			_expirationInterval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxExpirationInSeconds", 900));
		}

		public string AddSubscription(Subscription subscription, string connectionId)
		{
			_beforeSubscribe.Invoke(subscription);

			var route = subscription.Route();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
					RouteToGroupName(route));

			_signalR.AddConnectionToGroup(RouteToGroupName(route), connectionId);

			_tracer.SubscriptionAdded(subscription, connectionId);

            return route;
		}

		public void RemoveSubscription(string route, string connectionId)
		{
			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
					RouteToGroupName(route));

			_signalR.RemoveConnectionFromGroup(RouteToGroupName(route), connectionId);
		}
		
		[MessageBrokerUnitOfWork]
		public virtual IEnumerable<Message> PopMessages(string route, string mailboxId)
		{
			var mailbox = _mailboxRepository.Load(Guid.Parse(mailboxId));
			if (mailbox == null)
			{
				_mailboxRepository.Persist(new Mailbox
				{
					Route = route,
					Id = Guid.Parse(mailboxId),
					ExpiresAt = _now.UtcDateTime().Add(_expirationInterval)
				});
				return Enumerable.Empty<Message>();
			}

			var messages = mailbox.PopAllMessages();
			var updateExpirationAt = mailbox.ExpiresAt.Subtract(new TimeSpan(_expirationInterval.Ticks / 2));

			var update = _now.UtcDateTime() >= updateExpirationAt || messages.Any();

            if (update)
			{
				mailbox.ExpiresAt = _now.UtcDateTime().Add(_expirationInterval);
				_mailboxRepository.Persist(mailbox);
			}

			return messages;
		}

		[MessageBrokerUnitOfWork]
		public virtual void NotifyClients(Message message)
		{
			var routes = message.Routes();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New notification from client with (DomainUpdateType: {0}) (Routes: {1}) (Id: {2}).",
					message.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName)));

			purgeSometimes();

			_mailboxRepository.Load(routes)
				.ForEach(mailbox =>
				{
					mailbox.AddMessage(message);
					_mailboxRepository.Persist(mailbox);
				});

			foreach (var route in routes)
			{
				var r = route;
				_actionScheduler.Do(() => _signalR.CallOnEventMessage(RouteToGroupName(r), r, message));
			}

			_tracer.ClientsNotified(message);
        }

		private void purgeSometimes()
		{
			if (_now.UtcDateTime() < _nextPurge) return;
			_nextPurge = _now.UtcDateTime().AddMinutes(5); 
			_mailboxRepository.Purge();
		}

		public void NotifyClientsMultiple(IEnumerable<Message> notifications)
		{
			foreach (var notification in notifications)
			{
				NotifyClients(notification);
			}
		}

		public static string RouteToGroupName(string route)
		{
			//gethashcode won't work in 100% of the cases...
			UInt64 hashedValue = 3074457345618258791ul;
			for (int i = 0; i < route.Length; i++)
			{
				hashedValue += route[i];
				hashedValue *= 3074457345618258799ul;
			}
			return hashedValue.GetHashCode().ToString(CultureInfo.InvariantCulture);
		}

	}
}