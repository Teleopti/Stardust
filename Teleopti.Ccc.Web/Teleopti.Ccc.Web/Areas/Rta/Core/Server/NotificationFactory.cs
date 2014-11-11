using System;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class NotificationFactory
	{
		public static Notification CreateNotification(IActualAgentState actualAgentState)
		{
			var type = typeof(IActualAgentState);
			var notification = new Notification
				{
					StartDate = Subscription.DateToString(actualAgentState.ReceivedTime),
					EndDate = Subscription.DateToString(actualAgentState.ReceivedTime),
					DomainId = Subscription.IdToString(actualAgentState.PersonId),
					DomainType = type.Name,
					DomainQualifiedType = type.AssemblyQualifiedName,
					ModuleId = Subscription.IdToString(Guid.Empty),
					DomainUpdateType = (int)DomainUpdateType.Insert,
					BusinessUnitId = Subscription.IdToString(actualAgentState.BusinessUnit)
				};
			notification.SeralizeActualAgentState(actualAgentState);
			return notification;
		}


	}
}