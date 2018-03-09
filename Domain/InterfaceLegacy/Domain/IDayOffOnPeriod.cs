using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IDayOffOnPeriod
	{
		DateOnlyPeriod Period { get; }
		int DaysOffCount { get; }
		IScheduleDay FindBestSpotForDayOff(IHasContractDayOffDefinition hasContractDayOffDefinition, IScheduleDayAvailableForDayOffSpecification dayAvailableForDayOffSpecification, IEffectiveRestrictionCreator effectiveRestrictionCreator, SchedulingOptions schedulingOptions);
	}
}
