using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class CascadingResourceCalculationContextFactory : ResourceCalculationContextFactory
	{
		public CascadingResourceCalculationContextFactory(Func<ISchedulerStateHolder> schedulerStateHolder, CascadingPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard)
			:base(schedulerStateHolder, () => personSkillProvider, timeZoneGuard)
		{	
		}
	}
}