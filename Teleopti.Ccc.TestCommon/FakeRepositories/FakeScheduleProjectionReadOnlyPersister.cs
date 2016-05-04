using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleProjectionReadOnlyPersister : IScheduleProjectionReadOnlyPersister
	{
		private readonly IDictionary<Guid, DateTime> store = new Dictionary<Guid, DateTime>();
		private readonly IList<ScheduleProjectionReadOnlyModel> _data = new List<ScheduleProjectionReadOnlyModel>();

		public void SetNextActivityStartTime(IPerson person, DateTime time)
		{
			store.Add(person.Id.GetValueOrDefault(),time);
		}

		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public void AddProjectedLayer(ScheduleProjectionReadOnlyModel model)
		{
			_data.Add(model);
		}

		public void ClearDayForPerson(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp)
		{
		}
		
		public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			return _data.Where(x => x.PersonId == personId).ToArray();
		}

		public bool IsInitialized()
		{
			throw new NotImplementedException();
		}
	}
}