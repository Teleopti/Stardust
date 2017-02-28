using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IDayOffOnPeriod
	{
		DateOnlyPeriod Period { get; }
		IList<IScheduleDay> ScheduleDays { get; }
		int DaysOffCount { get; }
		IScheduleDay FindBestSpotForDayOff(IHasContractDayOffDefinition hasContractDayOffDefinition, IScheduleDayAvailableForDayOffSpecification dayAvailableForDayOffSpecification, IEffectiveRestrictionCreator effectiveRestrictionCreator, ISchedulingOptions schedulingOptions);
	}
}
