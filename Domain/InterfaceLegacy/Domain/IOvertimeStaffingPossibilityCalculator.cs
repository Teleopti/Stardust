using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeStaffingPossibilityCalculator
	{
		IList<CalculatedPossibilityModel> CalculateIntradayIntervalPossibilities(IPerson person, DateOnlyPeriod period, bool satisfyAllSkills);
	}
}