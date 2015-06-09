using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	public class MessageBrokerServer : IMessageBrokerServer
	{
		private readonly IActionScheduler _actionScheduler;
		private readonly ISignalR _signalR;
		private readonly IMailboxRepository _mailboxRepository;
		private readonly IBeforeSubscribe _beforeSubscribe;
		public ILog Logger = LogManager.GetLogger(typeof (MessageBrokerServer));

		public MessageBrokerServer(
			IActionScheduler actionScheduler,
			ISignalR signalR,
			IBeforeSubscribe beforeSubscribe,
			IMailboxRepository mailboxRepository)
		{
			_actionScheduler = actionScheduler;
			_signalR = signalR;
			_mailboxRepository = mailboxRepository;
			_beforeSubscribe = beforeSubscribe ?? new SubscriptionPassThrough();
		}

		public string AddSubscription(Subscription subscription, string connectionId)
		{
			_beforeSubscribe.Invoke(subscription);

			var route = subscription.Route();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
					RouteToGroupName(route));

			_signalR.AddConnectionToGroup(RouteToGroupName(route), connectionId);

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
		public virtual void AddMailbox(Subscription subscription)
		{
			_mailboxRepository.Persist(new Mailbox {Route = subscription.Route(), Id = Guid.Parse(subscription.MailboxId)});
		}

		[MessageBrokerUnitOfWork]
		public virtual IEnumerable<Message> PopMessages(string mailboxId)
		{
			var mailbox = _mailboxRepository.Load(Guid.Parse(mailboxId));
			var result = mailbox.PopAllMessages();
			if (result.Any())
				_mailboxRepository.Persist(mailbox);
			return result;
		}

		[MessageBrokerUnitOfWork]
		public virtual void NotifyClients(Message message)
		{
			var routes = message.Routes();

			if (Logger.IsDebugEnabled)
				Logger.DebugFormat("New notification from client with (DomainUpdateType: {0}) (Routes: {1}) (Id: {2}).",
					message.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName)));

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