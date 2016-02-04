using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ScheduleChangedEventPublisher : IPersistCallback
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly INow _now;

		public ScheduleChangedEventPublisher(IEventPopulatingPublisher eventPublisher, INow now)
		{
			_eventPublisher = eventPublisher;
			_now = now;
		}

		public void AfterFlush(IEnumerable<IRootChangeInfo> modifiedRoots)
		{
			var roots = modifiedRoots.Select(r => r.Root);
			var personAssignments = roots.OfType<IPersonAssignment>().Cast<IPersistableScheduleData>();
			var personAbsences = roots.OfType<IPersonAbsence>();
			var scheduleData = personAssignments.Concat(personAbsences)
				.Where(x => x.Scenario != null)
				.Select(x =>
				{
					if (x is IAggregateRootWithEvents)
						(x as IAggregateRootWithEvents).PopAllEvents(_now);
					return x;
				})
				.ToArray();
			
			if (!scheduleData.Any()) return;

			var scheduleChangesPerPerson = scheduleData
				.GroupBy(x => new
				{
					x.Person,
					x.Scenario
				}, x => x);

			var messages = scheduleChangesPerPerson
				.Select(g =>
				{
					return new ScheduleChangedEvent
					{
						PersonId = g.Key.Person.Id.GetValueOrDefault(),
						ScenarioId = g.Key.Scenario.Id.GetValueOrDefault(),
						StartDateTime = g.Min(s => s.Period.StartDateTime),
						EndDateTime = g.Max(s => s.Period.EndDateTime),
					};
				})
				.ToArray();

			if (!messages.Any()) return;

			var retries = 0;
			while (retries < 2)
			{
				try
				{
					retries++;
					_eventPublisher.Publish(messages);
					return;
				}
				catch (SqlException ex)
				{
					LogManager.GetLogger(typeof(ScheduleChangedEventPublisher)).Error(ex);
				}
			}
		}
		
	}
}