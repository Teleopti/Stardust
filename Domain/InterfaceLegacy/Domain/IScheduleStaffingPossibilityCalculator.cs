using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IScheduleStaffingPossibilityCalculator
	{
		IList<CalculatedPossibilityModel> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period);

		IList<CalculatedPossibilityModel> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period);
	}
}
