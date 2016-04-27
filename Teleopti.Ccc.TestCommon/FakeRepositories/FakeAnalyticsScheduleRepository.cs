﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		private readonly IList<IAnalyticsGeneric> fakeScenarios;
		private readonly IList<IAnalyticsAbsence> fakeAbsences;
		private readonly IList<IAnalyticsGeneric> fakeShiftCategories;
		public FakeAnalyticsScheduleRepository()
		{
			fakeScenarios = new List<IAnalyticsGeneric>
			{
				new AnalyticsGeneric {Code = Guid.Empty, Id = -1},
				new AnalyticsGeneric {Code = Guid.NewGuid(), Id = 1}
			};
			fakeAbsences = new List<IAnalyticsAbsence>
			{
				new AnalyticsAbsence { AbsenceCode  = Guid.Empty, AbsenceId = -1},
				new AnalyticsAbsence {AbsenceCode = Guid.NewGuid(), AbsenceId = 1}
			};
			fakeShiftCategories = new List<IAnalyticsGeneric>
			{
				new AnalyticsGeneric {Code = Guid.Empty, Id = -1},
				new AnalyticsGeneric {Code = Guid.NewGuid(), Id = 1}
			};
		}

		public void AddScenario(IAnalyticsGeneric scenario)
		{
			fakeScenarios.Add(scenario);
		}

		public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
		{
			throw new NotImplementedException();
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			throw new NotImplementedException();
		}

		public void DeleteFactSchedule(int date, int personId, int scenarioId)
		{
			throw new NotImplementedException();
		}

		public IList<IAnalyticsActivity> Activities()
		{
			return new IAnalyticsActivity[] {
				new AnalyticsActivity { ActivityCode = Guid.NewGuid(), ActivityId = 1, InPaidTime = true, InReadyTime = true }
			};
		}

		public IList<IAnalyticsAbsence> Absences()
		{
			return fakeAbsences;
		}

		public IList<IAnalyticsGeneric> Scenarios()
		{
			return fakeScenarios;
		}

		public IList<IAnalyticsGeneric> ShiftCategories()
		{
			return fakeShiftCategories;
		}

		public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
		{
			throw new NotImplementedException();
		}

		public IList<IAnalyticsGeneric> Overtimes()
		{
			throw new NotImplementedException();
		}

		public IList<IAnalyticsShiftLength> ShiftLengths()
		{
			throw new NotImplementedException();
		}

		public int ShiftLengthId(int shiftLength)
		{
			throw new NotImplementedException();
		}

		public void InsertStageScheduleChangedServicebus(DateOnly date, Guid personId, Guid scenarioId, Guid businessUnitId,
			DateTime datasourceUpdateDate)
		{
			throw new NotImplementedException();
		}
	}
}