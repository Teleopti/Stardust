using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStaffingPossibilityCalculator
	{
		CalculatedPossibilityModel CalculateIntradayAbsenceIntervalPossibilities();

		IList<CalculatedPossibilityModel> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period);

		CalculatedPossibilityModel CalculateIntradayOvertimeIntervalPossibilities();

		IList<CalculatedPossibilityModel> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period);
	}
}
