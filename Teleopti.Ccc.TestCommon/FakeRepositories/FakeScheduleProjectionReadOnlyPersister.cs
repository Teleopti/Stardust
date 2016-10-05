using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NHibernate.Util;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleProjectionReadOnlyPersister : IScheduleProjectionReadOnlyPersister
	{
		private readonly IList<ScheduleProjectionReadOnlyModel> _data = new List<ScheduleProjectionReadOnlyModel>();
		private int _numberOfHeadCounts;

		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup,
			IScenario scenario)
		{
			return _data.Where(d => d.ScenarioId == scenario.Id && d.StartDateTime <= period.StartDate.Date
									&& d.EndDateTime >= period.EndDate.Date).Select(d => new PayloadWorkTime
									{
										TotalContractTime = d.ContractTime.Ticks
									});
		}

		public void AddActivity(ScheduleProjectionReadOnlyModel model)
		{
			_data.Add(model);
			Console.Write(_data.Count);
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

		public IEnumerable<ScheduledActivity> ForPerson(DateOnly from, DateOnly to, Guid personId)
		{
			Console.Write(_data.Count);
			return (
				from l in _data
				where
					l.PersonId == personId &&
					l.BelongsToDate >= @from &&
					l.BelongsToDate <= to
				select JsonConvert.DeserializeObject<ScheduledActivity>(JsonConvert.SerializeObject(l))
				).ToList();
		}

		public IEnumerable<ScheduledActivity> ForPersons(DateOnly from, DateOnly to, IEnumerable<Guid> personIds)
		{
			Console.Write(_data.Count);
			return (
				from l in _data
				where
					personIds.Contains(l.PersonId) &&
					l.BelongsToDate >= @from &&
					l.BelongsToDate <= to
				select JsonConvert.DeserializeObject<ScheduledActivity>(JsonConvert.SerializeObject(l))
				).ToList();
		}

		public bool IsInitialized()
		{
			return _data.Any();
		}

		public void Clear()
		{
			_data.Clear();
		}
		public void Clear(Guid personId)
		{
			var toRemove = _data.Where(x => x.PersonId == personId).ToList();
			toRemove.ForEach(x => _data.Remove(x));
		}
	}
}