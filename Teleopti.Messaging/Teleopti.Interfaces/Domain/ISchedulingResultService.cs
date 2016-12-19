namespace Teleopti.Interfaces.Domain
{
    public interface ISchedulingResultService
    {
		//ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodWithSchedules, IResourceCalculationData resourceCalculationData = null);
		ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, IResourceCalculationData resourceCalculationData = null, bool emptyCache=true);
    }
}