using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleMessageSender : IMessageSender
	{
		private readonly IServiceBusSender _serviceBusSender;
		private static readonly Type[] IncludedTypes = new[] { typeof(IPersonAbsence), typeof(IPersonAssignment) };

		public ScheduleMessageSender(IServiceBusSender serviceBusSender)
		{
			_serviceBusSender = serviceBusSender;
		}

		public void Execute(IMessageBrokerIdentifier messageBrokerIdentifier, IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			if (!_serviceBusSender.EnsureBus()) return;

			var scheduleData = extractScheduleChangesOnly(modifiedRoots);
			if (!scheduleData.Any()) return;

			var people = scheduleData.Select(s => s.Person).Distinct();
			var scenarios = scheduleData.Select(s => s.Scenario).Distinct();

			foreach (var person in people)
			{
				foreach (var scenario in scenarios)
				{
					if (scenario==null) continue;

					var matchedItems = scheduleData.Where(s => s.Scenario!=null && s.Scenario.Equals(scenario) && s.Person.Equals(person));
					if (!matchedItems.Any()) continue;

					var startDateTime = matchedItems.Min(s => s.Period.StartDateTime);
					var endDateTime = matchedItems.Max(s => s.Period.EndDateTime);

					var message = new ScheduleChangedEvent
					              	{
										InitiatorId = messageBrokerIdentifier.InstanceId,
					              		ScenarioId = scenario.Id.GetValueOrDefault(),
										StartDateTime = startDateTime,
					              		EndDateTime = endDateTime,
					              		PersonId = person.Id.GetValueOrDefault(),
					              	};
					_serviceBusSender.Send(message);
				}
			}
		}

		private static IEnumerable<IPersistableScheduleData> extractScheduleChangesOnly(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = modifiedRoots.Select(r => r.Root).OfType<IPersistableScheduleData>();
			scheduleData = scheduleData.Where(s => IncludedTypes.Any(t => s.GetType().GetInterfaces().Contains(t)));
			return scheduleData;
		}
	}
}