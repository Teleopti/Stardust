using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStaffingPossibilityCalculator
	{
		IDictionary<DateTime, int> CalcuateIntradayAbsenceIntervalPossibilities();

		IDictionary<DateTime, int> CalcuateIntradayOvertimeIntervalPossibilities();
	}
}
