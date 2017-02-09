using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IScheduleStaffingPossibilityCalculator
	{
		IDictionary<DateTime, int> CalcuateIntradayAbsenceIntervalPossibilities();

		IDictionary<DateTime, int> CalcuateIntradayOvertimeIntervalPossibilities();
	}
}
