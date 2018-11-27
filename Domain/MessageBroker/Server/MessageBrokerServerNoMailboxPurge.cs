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
	public class MessageBrokerServerNoMailboxPurge : IMessageBrokerServer
	{
		private readonly ISignalR _signalR;
		private readonly IMailboxRepository _mailboxRepository;
		private readonly INow _now;
		private readonly IBeforeSubscribe _beforeSubscribe;
		private readonly TimeSpan _expirationInterval;
		private readonly MessageBrokerTracer _tracer = new MessageBrokerTracer();
		private readonly ILog _logger = LogManager.GetLogger(typeof(MessageBrokerServerNoMailboxPurge));

		public MessageBrokerServerNoMailboxPurge(
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
		}

		public string AddSubscription(Subscription subscription, string connectionId)
		{
			_beforeSubscribe.Invoke(subscription);

			var route = subscription.Route();

			if (_logger.IsDebugEnabled)
				_logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
					RouteToGroupName(route));

			_signalR.AddConnectionToGroup(RouteToGroupName(route), connectionId);

			_tracer.SubscriptionAdded(subscription, connectionId);

			return route;
		}

		public void RemoveSubscription(string route, string connectionId)
		{
			if (_logger.IsDebugEnabled)
				_logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
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

			if (_logger.IsDebugEnabled)
				_logger.DebugFormat("New notification from client with (DomainUpdateType: {0}) (Routes: {1}) (Id: {2}).",
					message.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName)));

			_mailboxRepository.AddMessage(message);

			foreach (var route in routes)
			{
				_signalR.CallOnEventMessage(RouteToGroupName(route), route, message);
			}

			_tracer.ClientsNotified(message);
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