using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ResourceCalculationContextFactoryExtensions
	{
		public static IDisposable Create(this ResourceCalculationContextFactory resourceCalculationContextFactory, ISchedulerStateHolder stateHolder, bool primarySkillMode, DateOnlyPeriod period)
		{
			return resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.SchedulingResultState.Skills, primarySkillMode, period);
		}
	}
}