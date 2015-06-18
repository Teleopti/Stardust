using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class MessageBrokerExtensions
	{
		private static subscriptionContext getContext()
		{
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity;
			var businessUnitId = identity.BusinessUnit.Id.GetValueOrDefault();
			var datasource = identity.DataSource.DataSourceName;

			return new subscriptionContext {BusinessUnitId = businessUnitId, Datasource = datasource};
		}

		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			var detail = getContext();
			broker.addSubscription(eventMessageHandler, detail.Datasource, detail.BusinessUnitId, null, null, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			var detail = getContext();
			broker.addSubscription(eventMessageHandler, detail.Datasource, detail.BusinessUnitId, null, null, null, domainObjectType, startDate, endDate);
		}

		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler,Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate, bool base64BinaryData = true, bool mailBox = false)
		{
			var detail = getContext();
			broker.addSubscription(eventMessageHandler, detail.Datasource, detail.BusinessUnitId, referenceObjectId, referenceObjectType, null, domainObjectType, startDate, endDate, base64BinaryData, mailBox);
		}

		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			var detail = getContext();
			broker.addSubscription(eventMessageHandler, detail.Datasource, detail.BusinessUnitId, null, null, domainObjectId, domainObjectType, startDate, endDate);
		}

		private class subscriptionContext
		{
			public string Datasource { get; set; }
			public Guid BusinessUnitId { get; set; }
		}




		public static void RegisterSubscription(this IMessageListener broker, string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, bool base64BinaryData = true, bool mailbox = false)
		{
			broker.addSubscription(eventMessageHandler, dataSource, businessUnitId, null, null, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public static void RegisterSubscription(this IMessageListener broker, string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType)
		{
			broker.addSubscription(eventMessageHandler, dataSource, businessUnitId, referenceObjectId, referenceObjectType, null, domainObjectType, Consts.MinDate, Consts.MaxDate);
		}

		public static void RegisterSubscription(this IMessageListener broker, string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			broker.addSubscription(eventMessageHandler, dataSource, businessUnitId, null, null, null, domainObjectType, startDate, endDate);
		}

		public static void RegisterSubscription(this IMessageListener broker, string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			broker.addSubscription(eventMessageHandler, dataSource, businessUnitId, null, null, domainObjectId, domainObjectType, startDate, endDate);
		}

		public static void RegisterSubscription(this IMessageListener broker, string dataSource, Guid businessUnitId, EventHandler<EventMessageArgs> eventMessageHandler, Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			broker.addSubscription(eventMessageHandler, dataSource, businessUnitId, referenceObjectId, referenceObjectType, null, domainObjectType, startDate, endDate);
		}

		private static void addSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, string datasource, Guid businessUnitId, Guid? referenceObjectId, Type referenceObjectType, Guid? domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate, bool base64BinaryData = true, bool mailbox = false)
		{
			var subscription = new Subscription
			{
				DomainId = domainObjectId.HasValue ? Subscription.IdToString(domainObjectId.Value) : null,
				DomainType = domainObjectType.Name,
				DomainReferenceId = referenceObjectId.HasValue ? Subscription.IdToString(referenceObjectId.Value) : null,
				DomainReferenceType =
					(referenceObjectType == null) ? null : referenceObjectType.AssemblyQualifiedName,
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate),
				DataSource = datasource,
				BusinessUnitId = Subscription.IdToString(businessUnitId),
			};

			var handler = eventMessageHandler;
			if (base64BinaryData)
			{
				handler = (s, e) =>
				{
					if (!string.IsNullOrEmpty(e.InternalMessage.BinaryData))
						e.Message.DomainObject = Convert.FromBase64String(e.InternalMessage.BinaryData);
					e.InternalMessage.BinaryData = null;
					eventMessageHandler(s, e);
				};
			}
			broker.RegisterSubscription(subscription, handler);
		}
	}
}