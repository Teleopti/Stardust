using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public static class ResourceCalculationContextFactoryExtensions
	{
		public static IDisposable Create(this ResourceCalculationContextFactory resourceCalculationContextFactory, ISchedulingResultStateHolder stateHolder, bool primarySkillMode, DateOnlyPeriod period)
		{
			return resourceCalculationContextFactory.Create(stateHolder.Schedules, stateHolder.Skills, stateHolder.ExternalStaff, primarySkillMode, period);
		}
	}
}