using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedInDefaultScenarioNotification : IHandleEvent<MainShiftReplaceNotificationEvent>, IRunOnHangfire
	{
		private readonly IMessageBrokerComposite _messageBroker;

		public ScheduleChangedInDefaultScenarioNotification(IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(MainShiftReplaceNotificationEvent message)
		{
			var firstDate = message.StartDateTime;
			var lastDate = message.EndDateTime;
			_messageBroker.Send(
				message.LogOnDatasource, 
				message.LogOnBusinessUnitId, 
				firstDate, 
				lastDate, 
				Guid.Empty,
				message.PersonId, 
				typeof (Person), 
				Guid.Empty, 
				typeof (IScheduleChangedInDefaultScenario),
				DomainUpdateType.NotApplicable, 
				null,
				message.CommandId == Guid.Empty ? Guid.NewGuid(): message.CommandId);
		}
	}
}