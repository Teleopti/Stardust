using System;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Domain.Helper
{
	public static class MessageBrokerExtensions
	{
		private static BusinessUnitDetail GetDetail()
		{
			var identity = (ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity;
			var businessUnitId = identity.BusinessUnit.Id.GetValueOrDefault();
			var datasource = identity.DataSource.DataSourceName;

			return new BusinessUnitDetail {BusinessUnitId = businessUnitId, Datasource = datasource};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType)
		{
			var detail = GetDetail();
			broker.RegisterSubscription(detail.Datasource,detail.BusinessUnitId,eventMessageHandler,domainObjectType);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			var detail = GetDetail();
			broker.RegisterSubscription(detail.Datasource, detail.BusinessUnitId, eventMessageHandler, domainObjectType, startDate,endDate);
		}

		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler,Guid referenceObjectId, Type referenceObjectType, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			var detail = GetDetail();
			broker.RegisterSubscription(detail.Datasource, detail.BusinessUnitId, eventMessageHandler,referenceObjectId,referenceObjectType, domainObjectType, startDate, endDate);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void RegisterEventSubscription(this IMessageListener broker, EventHandler<EventMessageArgs> eventMessageHandler, Guid domainObjectId, Type domainObjectType, DateTime startDate, DateTime endDate)
		{
			var detail = GetDetail();
			broker.RegisterSubscription(detail.Datasource, detail.BusinessUnitId, eventMessageHandler, domainObjectId, domainObjectType, startDate, endDate);
		}

		private class BusinessUnitDetail
		{
			public string Datasource { get; set; }
			public Guid BusinessUnitId { get; set; }
		}
	}
}