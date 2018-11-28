using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker.Server
{
	public class MessageBrokerServerThrottleMessages : IMessageBrokerServer, IDisposable
	{
		private readonly ISignalR _signalR;
		private readonly IMailboxRepository _mailboxRepository;
		private readonly INow _now;
		private readonly ITime _time;
		private readonly IBeforeSubscribe _beforeSubscribe;
		private readonly TimeSpan _expirationInterval;
		private readonly MessageBrokerTracer _tracer = new MessageBrokerTracer();
		private readonly ILog _logger = LogManager.GetLogger(typeof(MessageBrokerServerThrottleMessages));

		private IDisposable _timer;
		private readonly object _timerLock = new object();
		private readonly ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();
		private readonly TimeSpan _messageDistributionInterval;

		public MessageBrokerServerThrottleMessages(
			ISignalR signalR,
			IBeforeSubscribe beforeSubscribe,
			IMailboxRepository mailboxRepository,
			IConfigReader config,
			INow now,
			ITime time)
		{
			_signalR = signalR;
			_mailboxRepository = mailboxRepository;
			_now = now;
			_time = time;
			_beforeSubscribe = beforeSubscribe ?? new SubscriptionPassThrough();
			//12 = 80 messages per second
			_messageDistributionInterval = TimeSpan.FromMilliseconds(config.ReadValue("MessageBrokerServerIntervalMilliseconds", 12));
			_expirationInterval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxExpirationInSeconds", 60 * 15));
		}

		public string AddSubscription(Subscription subscription, string connectionId)
		{
			_beforeSubscribe.Invoke(subscription);

			var route = subscription.Route();

			if (_logger.IsDebugEnabled)
				_logger.DebugFormat("New subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
					RouteToGroupName.Convert(route));

			_signalR.AddConnectionToGroup(RouteToGroupName.Convert(route), connectionId);

			_tracer.SubscriptionAdded(subscription, connectionId);

			return route;
		}

		public void RemoveSubscription(string route, string connectionId)
		{
			if (_logger.IsDebugEnabled)
				_logger.DebugFormat("Remove subscription from client {0} with route {1} (Id: {2}).", connectionId, route,
					RouteToGroupName.Convert(route));

			_signalR.RemoveConnectionFromGroup(RouteToGroupName.Convert(route), connectionId);
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

		public void NotifyClients(Message message)
		{
			ensureThrottle();
			_messageQueue.Enqueue(message);
		}

		public void NotifyClientsMultiple(IEnumerable<Message> messages) =>
			messages.ForEach(NotifyClients);


		private void ensureThrottle()
		{
			if (_timer == null)
				_timer = _time.StartTimerWithLock(distributeAMessage, _timerLock, _messageDistributionInterval);
		}

		private void distributeAMessage()
		{
			if (!_messageQueue.TryDequeue(out var message))
				return;

			var routes = message.Routes();

			if (_logger.IsDebugEnabled)
				_logger.DebugFormat("New notification from client with (DomainUpdateType: {0}) (Routes: {1}) (Id: {2}).",
					message.DomainUpdateType, string.Join(", ", routes),
					string.Join(", ", routes.Select(RouteToGroupName.Convert)));

			AddMessagesToMailbox(message);

			foreach (var route in routes)
			{
				_signalR.CallOnEventMessage(RouteToGroupName.Convert(route), route, message);
			}

			_tracer.ClientsNotified(message);
		}

		[MessageBrokerUnitOfWork]
		protected virtual void AddMessagesToMailbox(Message message) =>
			_mailboxRepository.AddMessage(message);

		public void Dispose() =>
			_timer?.Dispose();
	}
}