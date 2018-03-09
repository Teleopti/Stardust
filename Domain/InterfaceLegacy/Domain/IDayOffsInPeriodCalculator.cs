using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Interface for checking number off day offs scheduled
	/// </summary>
	public interface IDayOffsInPeriodCalculator
	{
		bool HasCorrectNumberOfDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod, out int targetDaysOff, out IList<IScheduleDay> dayOffsNow);

	
	    bool OutsideOrAtMinimumTargetDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod);

	    bool OutsideOrAtMaximumTargetDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod);

		IList<DayOffOnPeriod> WeekPeriodsSortedOnDayOff(IScheduleMatrixPro scheduleMatrixPro);
	}
}