using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class MessageBrokerServer : IMessageBrokerServer
	{
		private readonly ISignalR _signalR;
		private readonly IMailboxRepository _mailboxRepository;
		private readonly INow _now;
		private readonly IBeforeSubscribe _beforeSubscribe;
		public ILog Logger = LogManager.GetLogger(typeof(MessageBrokerServer));
		private readonly TimeSpan _expirationInterval;
		private readonly TimeSpan _purgeInterval;
		private DateTime _nextPurge;
		private readonly MessageBrokerTracer _tracer = new MessageBrokerTracer();

		public MessageBrokerServer(
			ISignalR signalR,
			IBeforeSubscribe beforeSubscribe,
			IMailboxRepository mailboxRepository,
			IConfigReader config,
			INow now)
		{
			_signalR = signalR;
			_mailboxRepository = mailboxRepository;
			_now = now;
			_beforeSubscribe = beforeSubscribe ?? new SubscriptionPassThrough();
			_expirationInterval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxExpirationInSeconds", 60 * 15));
			_purgeInterval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxPurgeIntervalInSeconds", 60 * 5));
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
			var mailboxIdGuid = Guid.Parse(mailboxId);
			var mailbox = _mailboxRepository.Load(mailboxIdGuid);
			if (mailbox == null)
			{
				mailbox = new Mailbox
				{
					Route = route,
					Id = mailboxIdGuid,
					ExpiresAt = _now.UtcDateTime().Add(_expirationInterval)
				};
				try
				{
					_mailboxRepository.Add(mailbox);
				}
				catch (Exception e) when (e.ContainsSqlViolationOfPrimaryKey())
				{
					return Enumerable.Empty<Message>();
				}
			}

			var updateExpirationAt = mailbox.ExpiresAt.Subtract(new TimeSpan(_expirationInterval.Ticks / 2));
			var updateExpiration = _now.UtcDateTime() >= updateExpirationAt;

			return updateExpiration
				? _mailboxRepository.PopMessages(mailboxIdGuid, _now.UtcDateTime().Add(_expirationInterval))
				: _mailboxRepository.PopMessages(mailboxIdGuid, null);
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

			_mailboxRepository.AddMessage(message);

			foreach (var route in routes)
			{
				_signalR.CallOnEventMessage(RouteToGroupName(route), route, message);
			}

			_tracer.ClientsNotified(message);
		}

		private void purgeSometimes()
		{
			if (_now.UtcDateTime() < _nextPurge) return;
			_nextPurge = _now.UtcDateTime().Add(_purgeInterval);
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