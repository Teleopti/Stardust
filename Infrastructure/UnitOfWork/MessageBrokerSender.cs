using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class MessageBrokerSender : ITransactionHook
	{
		private readonly Func<IMessageBrokerComposite> _messageBroker;
		private readonly ICurrentInitiatorIdentifier _initiatorIdentifier;
		private readonly ICurrentDataSource _dataSource;
		private readonly ICurrentBusinessUnit _businessUnit;
		private readonly EventMessageFactory _eventMessageFactory;

		public MessageBrokerSender(
			Func<IMessageBrokerComposite> messageBroker, 
			ICurrentInitiatorIdentifier initiatorIdentifier,
			ICurrentDataSource dataSource,
			ICurrentBusinessUnit businessUnit
			)
		{
			_messageBroker = messageBroker;
			_initiatorIdentifier = initiatorIdentifier;
			_dataSource = dataSource;
			_businessUnit = businessUnit;
			_eventMessageFactory = new EventMessageFactory();
		}

		public void AfterCompletion(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var identifier = _initiatorIdentifier.Current();
			var initiatorId = identifier == null ? Guid.Empty : identifier.InitiatorId;

			var messageBroker = _messageBroker();

			if (messageBroker == null || !messageBroker.IsAlive) return;

			var eventMessages = new List<IEventMessage>();
			foreach (var change in modifiedRoots)
			{
				if (!MessageFilterManager.Instance.HasType(change.Root.GetType())) continue;

				var rootBrokerConditions = change.Root as IAggregateRootBrokerConditions;
				if (rootBrokerConditions != null)
				{
					if (rootBrokerConditions.SendChangeOverMessageBroker())
						eventMessages.Add(createEventMessage(change, initiatorId));
					continue;
				}

				eventMessages.Add(createEventMessage(change, initiatorId));

				var provideCustomChangeInfo = change.Root as IProvideCustomChangeInfo;
				if (provideCustomChangeInfo == null) continue;

				var changes = provideCustomChangeInfo.CustomChanges(change.Status);
				eventMessages.AddRange(changes.Select(rootChangeInfo => createEventMessage(rootChangeInfo, initiatorId)));
			}
			if (eventMessages.Count > 0)
			{
				messageBroker.Send(
					_dataSource.CurrentName(), 
					_businessUnit.CurrentId().GetValueOrDefault(), 
					eventMessages.ToArray());
			}
		}

		private IEventMessage createEventMessage(IRootChangeInfo change, Guid moduleId)
		{
			var changeWithRoot = change.Root as IMainReference;
			var periodRoot = change.Root as IPeriodized;
			var rootId = extractId(change.Root);
			var rootType = change.Root.GetType();
			IEventMessage eventMessage;
			if (periodRoot == null)
			{
				if (changeWithRoot == null)
					eventMessage = _eventMessageFactory.CreateEventMessage(
						moduleId,
						rootId,
						rootType,
						change.Status);
				else
					eventMessage = _eventMessageFactory.CreateEventMessage(
						moduleId,
						changeWithRoot.MainRoot.Id.Value,
						rootId,
						rootType,
						change.Status);
			}
			else
			{
				var period = periodRoot.Period;
				if (changeWithRoot == null)
					eventMessage = _eventMessageFactory.CreateEventMessage(
						period.StartDateTime,
						period.EndDateTime,
						moduleId,
						rootId,
						rootType,
						change.Status);
				else
					eventMessage = _eventMessageFactory.CreateEventMessage(
						period.StartDateTime,
						period.EndDateTime,
						moduleId,
						changeWithRoot.MainRoot.Id.Value,
						rootId,
						rootType,
						change.Status);
			}
			return eventMessage;
		}

		private static Guid extractId(object root)
		{
			var entity = root as IEntity;
			if (entity != null) return entity.Id.GetValueOrDefault();

			var custom = root as ICustomChangedEntity;
			if (custom != null) return custom.Id.GetValueOrDefault();

			return Guid.Empty;
		}
	}
}