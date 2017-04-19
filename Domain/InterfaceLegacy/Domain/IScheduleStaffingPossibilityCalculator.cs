using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStaffingPossibilityCalculator
	{
		IDictionary<DateTime, int> CalculateIntradayAbsenceIntervalPossibilities();

		IDictionary<DateOnly, IDictionary<DateTime, int>> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period);

		IDictionary<DateTime, int> CalculateIntradayOvertimeIntervalPossibilities();

		IDictionary<DateOnly, IDictionary<DateTime, int>> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period);
	}
}
