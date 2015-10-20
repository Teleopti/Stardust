using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleChangedEventPublisher : IPersistCallback
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IBeforeSendEvents _clearEvents;
		private static readonly Type[] IncludedTypes = new[] { typeof(IPersonAbsence), typeof(IPersonAssignment) };

		public ScheduleChangedEventPublisher(IEventPopulatingPublisher eventPublisher, IBeforeSendEvents clearEvents)
		{
			_eventPublisher = eventPublisher;
			_clearEvents = clearEvents;
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var scheduleData = extractScheduleChangesOnly(modifiedRoots);
			if (!scheduleData.Any()) return;

			var people = scheduleData.Select(s => s.Person).Distinct();
			var scenarios = scheduleData.Select(s => s.Scenario).Distinct();
			var messages = new List<ScheduleChangedEvent>();

			foreach (var person in people)
			{
				foreach (var scenario in scenarios)
				{
					if (scenario==null) continue;

					var matchedItems = scheduleData.Where(s => s.Scenario!=null && s.Scenario.Equals(scenario) && s.Person.Equals(person)).ToArray();
					if (!matchedItems.Any()) continue;

					_clearEvents.Execute(matchedItems);
					var startDateTime = matchedItems.Min(s => s.Period.StartDateTime);
					var endDateTime = matchedItems.Max(s => s.Period.EndDateTime);

					messages.Add(new ScheduleChangedEvent
					              	{
					              		ScenarioId = scenario.Id.GetValueOrDefault(),
										StartDateTime = startDateTime,
					              		EndDateTime = endDateTime,
					              		PersonId = person.Id.GetValueOrDefault(),
					              	});
				}
			}

			if (messages.Any())
			{
				var retries = 0;
				while (retries < 2)
				{
					try
					{
						retries++;
						_eventPublisher.Publish(messages.ToArray());
						return;
					}
					catch (SqlException)
					{ }
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