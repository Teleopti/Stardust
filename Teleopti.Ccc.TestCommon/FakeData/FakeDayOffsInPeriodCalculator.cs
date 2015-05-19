using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeDayOffsInPeriodCalculator : IDayOffsInPeriodCalculator
	{
		private bool _hasCorrectDaysOff = true;

		public void SetCorrectDayOffValue()
		{
			_hasCorrectDaysOff = false;
		}
		public IList<IScheduleDay> CountDayOffsOnPeriod(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			throw new NotImplementedException();
		}

		public bool HasCorrectNumberOfDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod, out int targetDaysOff, out IList<IScheduleDay> dayOffsNow)
		{
			targetDaysOff = 0;
			dayOffsNow = null;
			return _hasCorrectDaysOff;
		}

		public bool OutsideOrAtMinimumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			throw new NotImplementedException();
		}

		public bool OutsideOrAtMaximumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			throw new NotImplementedException();
		}

		public IList<IDayOffOnPeriod> WeekPeriodsSortedOnDayOff(IScheduleMatrixPro scheduleMatrixPro)
		{
			throw new NotImplementedException();
		}

		public IDayOffOnPeriod CountDayOffsOnPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}
	}
}