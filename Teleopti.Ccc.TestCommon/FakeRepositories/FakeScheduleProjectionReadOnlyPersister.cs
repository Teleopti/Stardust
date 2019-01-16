﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleProjectionReadOnlyPersister : IScheduleProjectionReadOnlyPersister
	{
		private readonly List<ScheduleProjectionReadOnlyModel> _data = new List<ScheduleProjectionReadOnlyModel>();

		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup,
			IScenario scenario)
		{
			return _data.Where(d => d.ScenarioId == scenario.Id && d.BelongsToDate >= period.StartDate
									&& d.BelongsToDate <= period.EndDate).Select(d => new PayloadWorkTime
									{
										TotalContractTime = d.ContractTime.Ticks,
										BelongsToDate = d.BelongsToDate.Date,
										HeadCounts = 1
									});
		}

		public void AddActivity(IEnumerable<ScheduleProjectionReadOnlyModel> models)
		{
			_data.AddRange(models);
		}

		public bool BeginAddingSchedule(DateOnly date, Guid scenarioId, Guid personId, int version)
		{
			return true;
		}

		public int GetNumberOfAbsencesPerDayAndBudgetGroup(Guid budgetGroupId, DateOnly currentDate)
		{
			return 0;
		}

		public IEnumerable<ScheduleProjectionReadOnlyModel> ForPerson(DateOnly date, Guid personId, Guid scenarioId)
		{
			return _data.Where(x => x.PersonId == personId).ToArray();
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