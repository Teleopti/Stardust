using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleProjectionReadOnlyPersister : 
		IScheduleProjectionReadOnlyPersister, 
		IScheduleProjectionReadOnlyReader
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


		public IEnumerable<ScheduledActivity> GetCurrentSchedule(DateTime utcNow, Guid personId)
		{
			return (
				from l in _data
				where
					l.PersonId == personId &&
					l.BelongsToDate.Date >= utcNow.Date.AddDays(-1) &&
					l.BelongsToDate.Date <= utcNow.Date.AddDays(1)
				select JsonConvert.DeserializeObject<ScheduledActivity>(JsonConvert.SerializeObject(l))
				).ToList();
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedules(DateTime utcNow, IEnumerable<Guid> personIds)
		{
			return (
				from l in _data
				where
					personIds.Contains(l.PersonId) &&
					l.BelongsToDate.Date >= utcNow.Date.AddDays(-1) &&
					l.BelongsToDate.Date <= utcNow.Date.AddDays(1)
				select JsonConvert.DeserializeObject<ScheduledActivity>(JsonConvert.SerializeObject(l))
				).ToList();
		}

		public void Clear(Guid personId)
		{
			var toRemove = _data.Where(x => x.PersonId == personId).ToList();
			toRemove.ForEach(x => _data.Remove(x));
		}
	}
}