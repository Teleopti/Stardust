using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IAbsenceStaffingPossibilityCalculator
	{
		CalculatedPossibilityModelResult CalculateIntradayIntervalPossibilities(IPerson person, DateOnlyPeriod period);
	}
}