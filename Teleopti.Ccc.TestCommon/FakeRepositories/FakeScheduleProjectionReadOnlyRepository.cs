using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{

	public class FakeScheduleProjectionReadOnlyRepository : IScheduleProjectionReadOnlyRepository
	{
		private class ProjectedShift
		{
			public Guid Person { get; set; }
			public ProjectionChangedEventLayer Layer { get; set; }
		}

		private IDictionary<Guid, DateTime> store = new Dictionary<Guid, DateTime>();
		private IList<ProjectedShift> _data = new List<ProjectedShift>();

		public void SetNextActivityStartTime(IPerson person, DateTime time)
		{
			store.Add(person.Id.GetValueOrDefault(),time);
		}

		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public int ClearDayForPerson(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp)
		{
			return 1;
		}

		public int AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId, ProjectionChangedEventLayer layer, DateTime scheduleLoadedTimeStamp)
		{
			_data.Add(new ProjectedShift
			{
				Person = personId,
				Layer = layer
			});
			return 1;
		}

		public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
		{
			throw new NotImplementedException();
		}

		public bool IsInitialized()
		{
			throw new NotImplementedException();
		}

		public DateTime? GetNextActivityStartTime(DateTime dateTime, Guid personId)
		{
			DateTime foundStart;
			if (store.TryGetValue(personId, out foundStart))
				return foundStart;

			return null;
		}

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			return _data.Where(x => x.Person == personId).Select(x => x.Layer).ToArray();
		}

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnlyPeriod datePeriod, Guid personId, Guid scenarioId)
		{
			throw new NotImplementedException();
		}
	}
}