using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStaffingPossibilityCalculator
	{
		IDictionary<DateTime, int> CalcuateIntradayAbsenceIntervalPossibilities();

		IDictionary<DateOnly, IDictionary<DateTime, int>> CalcuateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period);

		IDictionary<DateTime, int> CalcuateIntradayOvertimeIntervalPossibilities();

		IDictionary<DateOnly, IDictionary<DateTime, int>> CalcuateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period);
	}
}
