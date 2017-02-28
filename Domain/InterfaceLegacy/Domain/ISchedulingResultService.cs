using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface ISchedulingResultService
    {
		//ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodWithSchedules, IResourceCalculationData resourceCalculationData = null);
		ISkillResourceCalculationPeriodDictionary SchedulingResult(DateTimePeriod periodToRecalculate, IResourceCalculationData resourceCalculationData = null, bool emptyCache=true);
    }
}