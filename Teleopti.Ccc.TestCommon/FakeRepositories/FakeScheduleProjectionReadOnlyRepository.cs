﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{

	public class FakeScheduleProjectionReadOnlyRepository : IScheduleProjectionReadOnlyRepository
	{
		private IDictionary<Guid, DateTime> store = new Dictionary<Guid, DateTime>();

		public void SetNextActivityStartTime(IPerson person, DateTime time)
		{
			store.Add(person.Id.GetValueOrDefault(),time);
		}

		public IEnumerable<PayloadWorkTime> AbsenceTimePerBudgetGroup(DateOnlyPeriod period, IBudgetGroup budgetGroup, IScenario scenario)
		{
			throw new NotImplementedException();
		}

		public void ClearDayForPerson(DateOnly date, Guid scenarioId, Guid personId, DateTime scheduleLoadedTimeStamp)
		{
			throw new NotImplementedException();
		}

		public void AddProjectedLayer(DateOnly belongsToDate, Guid scenarioId, Guid personId, ProjectionChangedEventLayer layer)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public IEnumerable<ProjectionChangedEventLayer> ForPerson(DateOnlyPeriod datePeriod, Guid personId, Guid scenarioId)
		{
			throw new NotImplementedException();
		}
	}
}