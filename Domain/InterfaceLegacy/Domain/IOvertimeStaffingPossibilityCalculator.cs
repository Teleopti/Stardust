using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOvertimeStaffingPossibilityCalculator
	{
		IList<CalculatedPossibilityModel> CalculateIntradayIntervalPossibilities(DateOnlyPeriod period, bool satisfyAllSkills);
	}
}