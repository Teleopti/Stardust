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
		private readonly IList<ScheduleProjectionReadOnlyModel> _data = new List<ScheduleProjectionReadOnlyModel>();
	    private int _numberOfHeadCounts;

        public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public void AddActivity(ScheduleProjectionReadOnlyModel model)
		{
			_data.Add(model);
		}

		public bool BeginAddingSchedule(DateOnly date, Guid scenarioId, Guid personId, int version)
		{
			return true;
		}
		
		public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
		{
			return _numberOfHeadCounts;
        }

	    public void SetNumberOfAbsencesPerDayAndBudgetGroup(int numberOfHeadCounts)
	    {
	        _numberOfHeadCounts = numberOfHeadCounts;

	    }


        public IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			return _data.Where(x => x.PersonId == personId).ToArray();
		}

		public bool IsInitialized()
		{
			return _data.Any();
		}
	}
}