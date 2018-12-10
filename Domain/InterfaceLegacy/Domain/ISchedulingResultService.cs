using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface ISchedulingResultService
    {
		ISkillResourceCalculationPeriodDictionary SchedulingResult(DateTimePeriod periodToRecalculate, ResourceCalculationData resourceCalculationData = null, bool emptyCache=true);
    }
}