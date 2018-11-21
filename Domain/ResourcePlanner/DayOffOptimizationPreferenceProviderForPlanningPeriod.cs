using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class DayOffOptimizationPreferenceProviderForPlanningPeriod : IDayOffOptimizationPreferenceProviderForPlanningPeriod
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public DayOffOptimizationPreferenceProviderForPlanningPeriod(IPlanningPeriodRepository planningPeriodRepository)
		{
			_planningPeriodRepository = planningPeriodRepository;
		}
		
		public IDayOffOptimizationPreferenceProvider Fetch(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			return new DayOffOptimizationPreferenceProviderUsingFilters(planningPeriod.PlanningGroup.Settings);
		}
	}
}