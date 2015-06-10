using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Composite
{
	public class MessageCreator : IMessageCreator
	{
		private const int MessagesPerBatch = 75; //Tried out maximum to around 85.
		private readonly IMessageSender _messageSender;
		private readonly IMessageFilterManager _messageFilterManager;

		public MessageCreator(IMessageSender messageSender, IMessageFilterManager messageFilterManager)
		{
			_messageSender = messageSender;
			_messageFilterManager = messageFilterManager;
		}

		private IEnumerable<Message> createNotifications(
			string dataSource, 
			string businessUnitId,
			DateTime eventStartDate, 
			DateTime eventEndDate, 
			Guid moduleId,
			Guid referenceObjectId, 
			Guid domainObjectId, 
			Type domainObjectType,
			DomainUpdateType updateType, 
			byte[] domainObject)
		{
			if (_messageFilterManager.HasType(domainObjectType))
			{
				var eventStartDateString = Subscription.DateToString(eventStartDate);
				var eventEndDateString = Subscription.DateToString(eventEndDate);
				var moduleIdString = Subscription.IdToString(moduleId);
				var domainObjectIdString = Subscription.IdToString(domainObjectId);
				var domainQualifiedTypeString = _messageFilterManager.LookupTypeToSend(domainObjectType);
				var domainReferenceIdString = Subscription.IdToString(referenceObjectId);
				var domainObjectString = (domainObject != null) ? Convert.ToBase64String(domainObject) : null;
				yield return new Message
				{
					StartDate = eventStartDateString,
					EndDate = eventEndDateString,
					DomainId = domainObjectIdString,
					DomainType = _messageFilterManager.LookupType(domainObjectType).Name,
					DomainQualifiedType = domainQualifiedTypeString,
					DomainReferenceId = domainReferenceIdString,
					ModuleId = moduleIdString,
					DomainUpdateType = (int)updateType,
					DataSource = dataSource,
					BusinessUnitId = businessUnitId,
					BinaryData = domainObjectString
				};
			}
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			var notificationList = createNotifications(
				dataSource,
				Subscription.IdToString(businessUnitId),
				eventStartDate,
				eventEndDate,
				moduleId,
				referenceObjectId,
				domainObjectId,
				domainObjectType,
				updateType,
				domainObject);
			_messageSender.SendMultiple(notificationList);
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId, Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			Send(dataSource, businessUnitId, eventStartDate, eventEndDate, moduleId, Guid.Empty, null, domainObjectId, domainObjectType, updateType, domainObject);
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			var notificationList = new List<Message>();
			var businessUnitIdString = Subscription.IdToString(businessUnitId);
			foreach (var eventMessage in eventMessages)
			{
				notificationList.AddRange(
					createNotifications(
					dataSource, businessUnitIdString, 
					eventMessage.EventStartDate,
					eventMessage.EventEndDate, 
					eventMessage.ModuleId,
					eventMessage.ReferenceObjectId, 
					eventMessage.DomainObjectId,
					eventMessage.DomainObjectTypeCache,
					eventMessage.DomainUpdateType,
					eventMessage.DomainObject));

				if (notificationList.Count > MessagesPerBatch)
				{
					_messageSender.SendMultiple(notificationList);
					notificationList.Clear();
				}
			}
			if (notificationList.Count > 0)
				_messageSender.SendMultiple(notificationList);
		}
	}
}