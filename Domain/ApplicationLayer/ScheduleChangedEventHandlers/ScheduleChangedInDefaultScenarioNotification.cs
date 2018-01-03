using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker.Client;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ScheduleChangedInDefaultScenarioNotification : 
		IHandleEvent<ProjectionChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IMessageBrokerComposite _messageBroker;

		public ScheduleChangedInDefaultScenarioNotification(IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void Handle(ProjectionChangedEvent message)
		{
			if (!message.ScheduleDays.Any() || !message.IsDefaultScenario)
				return;
			var firstDate = message.ScheduleDays.Min(d => d.Date).AddDays(1);
			var lastDate = message.ScheduleDays.Max(d => d.Date);
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