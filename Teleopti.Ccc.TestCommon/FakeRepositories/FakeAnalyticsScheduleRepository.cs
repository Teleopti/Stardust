﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsScheduleRepository : IAnalyticsScheduleRepository
	{
		public void PersistFactScheduleBatch(IList<IFactScheduleRow> factScheduleRows)
		{
			throw new NotImplementedException();
		}

		public void PersistFactScheduleDayCountRow(IAnalyticsFactScheduleDayCount dayCount)
		{
			throw new NotImplementedException();
		}

		public void DeleteFactSchedule(int dateId, int personId, int scenarioId)
		{
			throw new NotImplementedException();
		}

		public IAnalyticsPersonBusinessUnit PersonAndBusinessUnit(Guid personPeriodCode)
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

		public void UpdateUnlinkedPersonids(int[] personPeriodIds)
		{
			throw new NotImplementedException();
		}

		public int GetFactScheduleRowCount(int personId)
		{
			throw new NotImplementedException();
		}

		public int GetFactScheduleDayCountRowCount(int personId)
		{
			throw new NotImplementedException();
		}

		public int GetFactScheduleDeviationRowCount(int personId)
		{
			throw new NotImplementedException();
		}

		public void DeleteInvalidScheduleRows(int[] personPeriodIds)
		{
			throw new NotImplementedException();
		}
	}
}