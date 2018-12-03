using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceStaffingPossibilityCalculator
	{
		IList<CalculatedPossibilityModel> CalculateIntradayIntervalPossibilities(IPerson person, DateOnlyPeriod period);
	}
}