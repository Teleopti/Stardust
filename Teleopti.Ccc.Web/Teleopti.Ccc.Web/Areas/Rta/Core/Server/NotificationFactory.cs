using System;
using System.Text;
using Newtonsoft.Json;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client
{
	public class NotificationFactory
	{
		public static Notification CreateNotification(IActualAgentState actualAgentState)
		{
			var domainObject = JsonConvert.SerializeObject(actualAgentState);
			var type = typeof (IActualAgentState);

			var notification = new Notification
				{
					StartDate =
						Subscription.DateToString(actualAgentState.ReceivedTime.Add(actualAgentState.TimeInState.Negate())),
					EndDate = Subscription.DateToString(actualAgentState.ReceivedTime),
					DomainId = Subscription.IdToString(actualAgentState.PersonId),
					DomainType = type.Name,
					DomainQualifiedType = type.AssemblyQualifiedName,
					ModuleId = Subscription.IdToString(Guid.Empty),
					DomainUpdateType = (int) DomainUpdateType.Insert,
					BinaryData = Convert.ToBase64String(Encoding.UTF8.GetBytes(domainObject)),
					BusinessUnitId = Subscription.IdToString(actualAgentState.BusinessUnit)
				};
			return notification;
		}

	}
}